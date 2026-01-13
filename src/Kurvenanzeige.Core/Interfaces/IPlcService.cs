using Kurvenanzeige.Core.Configuration;
using Kurvenanzeige.Core.Models;

namespace Kurvenanzeige.Core.Interfaces;

public interface IPlcService
{
    Task<bool> ConnectAsync(CancellationToken ct = default);
    Task DisconnectAsync();
    bool IsConnected { get; }

    Task<float> ReadRealAsync(int dbNumber, int offset, CancellationToken ct = default);
    Task<int> ReadIntAsync(int dbNumber, int offset, CancellationToken ct = default);
    Task<bool> ReadBoolAsync(int dbNumber, int offset, int bit, CancellationToken ct = default);
    Task<byte[]> ReadBytesAsync(int dbNumber, int offset, int length, CancellationToken ct = default);

    Task<List<PlcDataPoint>> ReadMultipleAsync(List<DataPointConfig> configs, CancellationToken ct = default);
}
