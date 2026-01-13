using Kurvenanzeige.Core.Configuration;
using Kurvenanzeige.Core.Models;

namespace Kurvenanzeige.Core.Interfaces;

public interface IDataRepository
{
    Task<List<DataPointConfig>> GetEnabledDataPointsAsync();
    Task<DataPointConfig?> GetDataPointAsync(string tagName);
    Task SaveDataPointConfigAsync(DataPointConfig config);

    Task BulkInsertReadingsAsync(List<PlcDataPoint> readings, CancellationToken ct = default);

    Task<List<AnalogValue>> GetAnalogHistoryAsync(string tagName, DateTime from, DateTime to);
    Task<List<DigitalSignal>> GetDigitalHistoryAsync(string tagName, DateTime from, DateTime to);
    Task<Dictionary<string, PlcDataPoint>> GetLatestValuesAsync();

    Task DeleteOldReadingsAsync(DateTime olderThan);
}
