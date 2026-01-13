namespace Kurvenanzeige.Core.Interfaces;

public interface IDataArchiver
{
    Task CreateHourlyAggregatesAsync();
    Task CreateDailyAggregatesAsync();
    Task CleanupOldDataAsync();
}
