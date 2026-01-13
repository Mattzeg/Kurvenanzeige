using Kurvenanzeige.Core.Configuration;
using Kurvenanzeige.Core.Interfaces;
using Kurvenanzeige.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using S7.Net;
using System.Net.Sockets;

namespace Kurvenanzeige.Infrastructure.Plc;

public class S7PlcService : IPlcService, IDisposable
{
    private readonly PlcConnectionConfig _config;
    private readonly ILogger<S7PlcService> _logger;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private S7.Net.Plc? _plc;
    private int _reconnectAttempts = 0;
    private DateTime _lastConnectionAttempt = DateTime.MinValue;
    private bool _disposed = false;

    public S7PlcService(IOptions<PlcConnectionConfig> config, ILogger<S7PlcService> logger)
    {
        _config = config.Value;
        _logger = logger;
    }

    public bool IsConnected => _plc?.IsConnected ?? false;

    public async Task<bool> ConnectAsync(CancellationToken ct = default)
    {
        await _connectionLock.WaitAsync(ct);
        try
        {
            if (_plc != null && _plc.IsConnected)
                return true;

            var delay = TimeSpan.FromSeconds(Math.Min(60, Math.Pow(2, _reconnectAttempts)));
            if (DateTime.Now - _lastConnectionAttempt < delay)
            {
                _logger.LogDebug("Connection backoff active. Waiting {Delay}s before retry", delay.TotalSeconds);
                return false;
            }

            _lastConnectionAttempt = DateTime.Now;

            try
            {
                _logger.LogInformation("Connecting to PLC at {IpAddress} Rack={Rack} Slot={Slot}",
                    _config.IpAddress, _config.Rack, _config.Slot);

                CpuType cpuType = _config.CpuType switch
                {
                    "S71500" => CpuType.S71500,
                    "S71200" => CpuType.S71200,
                    "S7300" => CpuType.S7300,
                    "S7400" => CpuType.S7400,
                    _ => CpuType.S71500
                };

                _plc = new S7.Net.Plc(cpuType, _config.IpAddress, (short)_config.Rack, (short)_config.Slot);

                await Task.Run(() => _plc.Open(), ct);

                if (_plc.IsConnected)
                {
                    _logger.LogInformation("Successfully connected to PLC");
                    _reconnectAttempts = 0;
                    return true;
                }

                _logger.LogWarning("Failed to connect to PLC");
                _reconnectAttempts++;
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to PLC");
                _reconnectAttempts++;

                if (_reconnectAttempts >= _config.MaxReconnectAttempts)
                {
                    _logger.LogError("Max reconnect attempts ({Max}) reached", _config.MaxReconnectAttempts);
                }

                return false;
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task DisconnectAsync()
    {
        await _connectionLock.WaitAsync();
        try
        {
            if (_plc != null)
            {
                _plc.Close();
                _plc = null;
                _logger.LogInformation("Disconnected from PLC");
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task<float> ReadRealAsync(int dbNumber, int offset, CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);

        try
        {
            var result = await Task.Run(() => _plc!.Read($"DB{dbNumber}.DBD{offset}"), ct);
            return Convert.ToSingle(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading REAL from DB{DbNumber}.DBD{Offset}", dbNumber, offset);
            throw new PlcReadException($"Failed to read REAL from DB{dbNumber}.DBD{offset}", ex);
        }
    }

    public async Task<int> ReadIntAsync(int dbNumber, int offset, CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);

        try
        {
            var result = await Task.Run(() => _plc!.Read($"DB{dbNumber}.DBW{offset}"), ct);
            return Convert.ToInt32(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading INT from DB{DbNumber}.DBW{Offset}", dbNumber, offset);
            throw new PlcReadException($"Failed to read INT from DB{dbNumber}.DBW{offset}", ex);
        }
    }

    public async Task<bool> ReadBoolAsync(int dbNumber, int offset, int bit, CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);

        try
        {
            var result = await Task.Run(() => _plc!.Read($"DB{dbNumber}.DBX{offset}.{bit}"), ct);
            return Convert.ToBoolean(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading BOOL from DB{DbNumber}.DBX{Offset}.{Bit}", dbNumber, offset, bit);
            throw new PlcReadException($"Failed to read BOOL from DB{dbNumber}.DBX{offset}.{bit}", ex);
        }
    }

    public async Task<byte[]> ReadBytesAsync(int dbNumber, int offset, int length, CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);

        try
        {
            var bytes = await Task.Run(() => _plc!.ReadBytes(DataType.DataBlock, dbNumber, offset, length), ct);
            return bytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading bytes from DB{DbNumber} Offset={Offset} Length={Length}",
                dbNumber, offset, length);
            throw new PlcReadException($"Failed to read bytes from DB{dbNumber}", ex);
        }
    }

    public async Task<string> ReadStringAsync(int dbNumber, int offset, int maxLength = 254, CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);

        try
        {
            // S7 String format: Byte 0 = max length, Byte 1 = actual length, Byte 2+ = characters
            var bytes = await ReadBytesAsync(dbNumber, offset, maxLength + 2, ct);
            var actualLength = bytes[1];

            if (actualLength > maxLength)
                actualLength = (byte)maxLength;

            var stringBytes = new byte[actualLength];
            Array.Copy(bytes, 2, stringBytes, 0, actualLength);

            return System.Text.Encoding.ASCII.GetString(stringBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading STRING from DB{DbNumber}.{Offset}", dbNumber, offset);
            throw new PlcReadException($"Failed to read STRING from DB{dbNumber}.{offset}", ex);
        }
    }

    public async Task<List<PlcDataPoint>> ReadMultipleAsync(List<DataPointConfig> configs, CancellationToken ct = default)
    {
        await EnsureConnectedAsync(ct);

        var results = new List<PlcDataPoint>();
        var timestamp = DateTime.Now;

        foreach (var config in configs)
        {
            try
            {
                PlcDataPoint? dataPoint = config.DataType switch
                {
                    DataPointType.Analog => await ReadAnalogValueAsync(config, timestamp, ct),
                    DataPointType.Digital => await ReadDigitalSignalAsync(config, timestamp, ct),
                    DataPointType.DataBlock => await ReadDataBlockValueAsync(config, timestamp, ct),
                    DataPointType.String => await ReadStringValueAsync(config, timestamp, ct),
                    _ => null
                };

                if (dataPoint != null)
                {
                    results.Add(dataPoint);
                }
            }
            catch (PlcReadException ex)
            {
                _logger.LogWarning(ex, "Failed to read tag {TagName}", config.TagName);
                results.Add(CreateErrorDataPoint(config, timestamp));
            }
        }

        return results;
    }

    private async Task<AnalogValue> ReadAnalogValueAsync(DataPointConfig config, DateTime timestamp, CancellationToken ct)
    {
        var value = await ReadRealAsync(config.DbNumber, config.Offset, ct);

        return new AnalogValue
        {
            TagName = config.TagName,
            DisplayName = config.DisplayName,
            DbNumber = config.DbNumber,
            Offset = config.Offset,
            Value = value,
            Unit = config.Unit,
            MinValue = config.MinValue,
            MaxValue = config.MaxValue,
            Quality = Quality.Good,
            Timestamp = timestamp
        };
    }

    private async Task<DigitalSignal> ReadDigitalSignalAsync(DataPointConfig config, DateTime timestamp, CancellationToken ct)
    {
        if (!config.Bit.HasValue)
        {
            throw new ArgumentException($"Bit position not specified for digital signal {config.TagName}");
        }

        var value = await ReadBoolAsync(config.DbNumber, config.Offset, config.Bit.Value, ct);

        return new DigitalSignal
        {
            TagName = config.TagName,
            DisplayName = config.DisplayName,
            DbNumber = config.DbNumber,
            Offset = config.Offset,
            Bit = config.Bit.Value,
            Value = value,
            Quality = Quality.Good,
            Timestamp = timestamp
        };
    }

    private async Task<DataBlockValue> ReadDataBlockValueAsync(DataPointConfig config, DateTime timestamp, CancellationToken ct)
    {
        var bytes = await ReadBytesAsync(config.DbNumber, config.Offset, 256, ct);

        return new DataBlockValue
        {
            TagName = config.TagName,
            DisplayName = config.DisplayName,
            DbNumber = config.DbNumber,
            Offset = config.Offset,
            RawData = bytes,
            StructureJson = Convert.ToBase64String(bytes),
            Quality = Quality.Good,
            Timestamp = timestamp
        };
    }

    private async Task<StringValue> ReadStringValueAsync(DataPointConfig config, DateTime timestamp, CancellationToken ct)
    {
        var maxLength = 254; // Default S7 string length
        var value = await ReadStringAsync(config.DbNumber, config.Offset, maxLength, ct);

        return new StringValue
        {
            TagName = config.TagName,
            DisplayName = config.DisplayName,
            DbNumber = config.DbNumber,
            Offset = config.Offset,
            Value = value,
            MaxLength = maxLength,
            Quality = Quality.Good,
            Timestamp = timestamp
        };
    }

    private PlcDataPoint CreateErrorDataPoint(DataPointConfig config, DateTime timestamp)
    {
        return config.DataType switch
        {
            DataPointType.Analog => new AnalogValue
            {
                TagName = config.TagName,
                DisplayName = config.DisplayName,
                DbNumber = config.DbNumber,
                Offset = config.Offset,
                Value = 0,
                Quality = Quality.Bad,
                Timestamp = timestamp
            },
            DataPointType.Digital => new DigitalSignal
            {
                TagName = config.TagName,
                DisplayName = config.DisplayName,
                DbNumber = config.DbNumber,
                Offset = config.Offset,
                Bit = config.Bit ?? 0,
                Value = false,
                Quality = Quality.Bad,
                Timestamp = timestamp
            },
            DataPointType.String => new StringValue
            {
                TagName = config.TagName,
                DisplayName = config.DisplayName,
                DbNumber = config.DbNumber,
                Offset = config.Offset,
                Value = string.Empty,
                Quality = Quality.Bad,
                Timestamp = timestamp
            },
            _ => throw new NotSupportedException($"Unsupported data type: {config.DataType}")
        };
    }

    private async Task EnsureConnectedAsync(CancellationToken ct)
    {
        if (!IsConnected)
        {
            var connected = await ConnectAsync(ct);
            if (!connected)
            {
                throw new PlcConnectionException("Not connected to PLC");
            }
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _plc?.Close();
        _plc = null;
        _connectionLock?.Dispose();
        _disposed = true;
    }
}

public class PlcConnectionException : Exception
{
    public PlcConnectionException(string message) : base(message) { }
    public PlcConnectionException(string message, Exception innerException) : base(message, innerException) { }
}

public class PlcReadException : Exception
{
    public PlcReadException(string message) : base(message) { }
    public PlcReadException(string message, Exception innerException) : base(message, innerException) { }
}
