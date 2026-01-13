using Kurvenanzeige.Core.Configuration;
using Kurvenanzeige.Core.Interfaces;
using Kurvenanzeige.Core.Models;
using Kurvenanzeige.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kurvenanzeige.Infrastructure.Data.Repositories;

public class DataRepository : IDataRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<DataRepository> _logger;
    private readonly Dictionary<string, PlcDataPoint> _latestValuesCache = new();
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    public DataRepository(AppDbContext context, ILogger<DataRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<DataPointConfig>> GetEnabledDataPointsAsync()
    {
        var configs = await _context.DataPointConfigurations
            .Where(c => c.IsEnabled)
            .AsNoTracking()
            .ToListAsync();

        return configs.Select(c => new DataPointConfig
        {
            TagName = c.TagName,
            DisplayName = c.DisplayName,
            DataType = Enum.Parse<DataPointType>(c.DataType),
            DbNumber = c.DbNumber,
            Offset = c.Offset,
            Bit = c.Bit,
            Unit = c.Unit,
            MinValue = c.MinValue,
            MaxValue = c.MaxValue,
            IsEnabled = c.IsEnabled,
            PollingInterval = c.PollingInterval
        }).ToList();
    }

    public async Task<List<DataPointConfig>> GetAllDataPointsAsync()
    {
        var configs = await _context.DataPointConfigurations
            .AsNoTracking()
            .ToListAsync();

        return configs.Select(c => new DataPointConfig
        {
            TagName = c.TagName,
            DisplayName = c.DisplayName,
            DataType = Enum.Parse<DataPointType>(c.DataType),
            DbNumber = c.DbNumber,
            Offset = c.Offset,
            Bit = c.Bit,
            Unit = c.Unit,
            MinValue = c.MinValue,
            MaxValue = c.MaxValue,
            IsEnabled = c.IsEnabled,
            PollingInterval = c.PollingInterval
        }).ToList();
    }

    public async Task<DataPointConfig?> GetDataPointAsync(string tagName)
    {
        var config = await _context.DataPointConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.TagName == tagName);

        if (config == null)
            return null;

        return new DataPointConfig
        {
            TagName = config.TagName,
            DisplayName = config.DisplayName,
            DataType = Enum.Parse<DataPointType>(config.DataType),
            DbNumber = config.DbNumber,
            Offset = config.Offset,
            Bit = config.Bit,
            Unit = config.Unit,
            MinValue = config.MinValue,
            MaxValue = config.MaxValue,
            IsEnabled = config.IsEnabled,
            PollingInterval = config.PollingInterval
        };
    }

    public async Task SaveDataPointConfigAsync(DataPointConfig config)
    {
        var existing = await _context.DataPointConfigurations
            .FirstOrDefaultAsync(c => c.TagName == config.TagName);

        if (existing != null)
        {
            existing.DisplayName = config.DisplayName;
            existing.DataType = config.DataType.ToString();
            existing.DbNumber = config.DbNumber;
            existing.Offset = config.Offset;
            existing.Bit = config.Bit;
            existing.Unit = config.Unit;
            existing.MinValue = config.MinValue;
            existing.MaxValue = config.MaxValue;
            existing.IsEnabled = config.IsEnabled;
            existing.PollingInterval = config.PollingInterval;
            existing.UpdatedAt = DateTime.Now;
        }
        else
        {
            var entity = new DataPointConfiguration
            {
                TagName = config.TagName,
                DisplayName = config.DisplayName,
                DataType = config.DataType.ToString(),
                DbNumber = config.DbNumber,
                Offset = config.Offset,
                Bit = config.Bit,
                Unit = config.Unit,
                MinValue = config.MinValue,
                MaxValue = config.MaxValue,
                IsEnabled = config.IsEnabled,
                PollingInterval = config.PollingInterval,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            await _context.DataPointConfigurations.AddAsync(entity);
        }

        await _context.SaveChangesAsync();
    }

    public async Task BulkInsertReadingsAsync(List<PlcDataPoint> readings, CancellationToken ct = default)
    {
        if (!readings.Any())
            return;

        try
        {
            foreach (var reading in readings)
            {
                switch (reading)
                {
                    case AnalogValue analog:
                        await _context.AnalogReadings.AddAsync(new AnalogReading
                        {
                            TagName = analog.TagName,
                            DbNumber = analog.DbNumber,
                            Offset = analog.Offset,
                            Value = analog.Value,
                            Unit = analog.Unit,
                            Quality = (int)analog.Quality,
                            Timestamp = analog.Timestamp
                        }, ct);
                        break;

                    case DigitalSignal digital:
                        await _context.DigitalReadings.AddAsync(new DigitalReading
                        {
                            TagName = digital.TagName,
                            DbNumber = digital.DbNumber,
                            Offset = digital.Offset,
                            Bit = digital.Bit,
                            Value = digital.Value,
                            Quality = (int)digital.Quality,
                            Timestamp = digital.Timestamp
                        }, ct);
                        break;

                    case DataBlockValue dataBlock:
                        await _context.DataBlockReadings.AddAsync(new DataBlockReading
                        {
                            TagName = dataBlock.TagName,
                            DbNumber = dataBlock.DbNumber,
                            StructureJson = dataBlock.StructureJson,
                            Quality = (int)dataBlock.Quality,
                            Timestamp = dataBlock.Timestamp
                        }, ct);
                        break;
                }
            }

            await _context.SaveChangesAsync(ct);

            await UpdateLatestValuesCacheAsync(readings);

            _logger.LogDebug("Inserted {Count} readings into database", readings.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting readings into database");
            throw;
        }
    }

    public async Task<List<AnalogValue>> GetAnalogHistoryAsync(string tagName, DateTime from, DateTime to)
    {
        var readings = await _context.AnalogReadings
            .Where(r => r.TagName == tagName && r.Timestamp >= from && r.Timestamp <= to)
            .OrderBy(r => r.Timestamp)
            .AsNoTracking()
            .ToListAsync();

        return readings.Select(r => new AnalogValue
        {
            TagName = r.TagName,
            DbNumber = r.DbNumber,
            Offset = r.Offset,
            Value = r.Value,
            Unit = r.Unit,
            Quality = (Quality)r.Quality,
            Timestamp = r.Timestamp
        }).ToList();
    }

    public async Task<List<DigitalSignal>> GetDigitalHistoryAsync(string tagName, DateTime from, DateTime to)
    {
        var readings = await _context.DigitalReadings
            .Where(r => r.TagName == tagName && r.Timestamp >= from && r.Timestamp <= to)
            .OrderBy(r => r.Timestamp)
            .AsNoTracking()
            .ToListAsync();

        return readings.Select(r => new DigitalSignal
        {
            TagName = r.TagName,
            DbNumber = r.DbNumber,
            Offset = r.Offset,
            Bit = r.Bit,
            Value = r.Value,
            Quality = (Quality)r.Quality,
            Timestamp = r.Timestamp
        }).ToList();
    }

    public async Task<Dictionary<string, PlcDataPoint>> GetLatestValuesAsync()
    {
        await _cacheLock.WaitAsync();
        try
        {
            if (_latestValuesCache.Any())
            {
                return new Dictionary<string, PlcDataPoint>(_latestValuesCache);
            }

            var latestValues = new Dictionary<string, PlcDataPoint>();

            var analogReadings = await _context.AnalogReadings
                .GroupBy(r => r.TagName)
                .Select(g => g.OrderByDescending(r => r.Timestamp).FirstOrDefault())
                .AsNoTracking()
                .ToListAsync();

            foreach (var reading in analogReadings.Where(r => r != null))
            {
                latestValues[reading!.TagName] = new AnalogValue
                {
                    TagName = reading.TagName,
                    DbNumber = reading.DbNumber,
                    Offset = reading.Offset,
                    Value = reading.Value,
                    Unit = reading.Unit,
                    Quality = (Quality)reading.Quality,
                    Timestamp = reading.Timestamp
                };
            }

            var digitalReadings = await _context.DigitalReadings
                .GroupBy(r => r.TagName)
                .Select(g => g.OrderByDescending(r => r.Timestamp).FirstOrDefault())
                .AsNoTracking()
                .ToListAsync();

            foreach (var reading in digitalReadings.Where(r => r != null))
            {
                latestValues[reading!.TagName] = new DigitalSignal
                {
                    TagName = reading.TagName,
                    DbNumber = reading.DbNumber,
                    Offset = reading.Offset,
                    Bit = reading.Bit,
                    Value = reading.Value,
                    Quality = (Quality)reading.Quality,
                    Timestamp = reading.Timestamp
                };
            }

            _latestValuesCache.Clear();
            foreach (var kvp in latestValues)
            {
                _latestValuesCache[kvp.Key] = kvp.Value;
            }

            return latestValues;
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    public async Task DeleteOldReadingsAsync(DateTime olderThan)
    {
        try
        {
            var deletedAnalog = await _context.AnalogReadings
                .Where(r => r.Timestamp < olderThan)
                .ExecuteDeleteAsync();

            var deletedDigital = await _context.DigitalReadings
                .Where(r => r.Timestamp < olderThan)
                .ExecuteDeleteAsync();

            var deletedDataBlock = await _context.DataBlockReadings
                .Where(r => r.Timestamp < olderThan)
                .ExecuteDeleteAsync();

            _logger.LogInformation("Deleted old readings: {Analog} analog, {Digital} digital, {DataBlock} datablock",
                deletedAnalog, deletedDigital, deletedDataBlock);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting old readings");
            throw;
        }
    }

    private async Task UpdateLatestValuesCacheAsync(List<PlcDataPoint> readings)
    {
        await _cacheLock.WaitAsync();
        try
        {
            foreach (var reading in readings)
            {
                _latestValuesCache[reading.TagName] = reading;
            }
        }
        finally
        {
            _cacheLock.Release();
        }
    }
}
