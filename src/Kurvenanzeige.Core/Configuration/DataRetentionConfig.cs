namespace Kurvenanzeige.Core.Configuration;

public class DataRetentionConfig
{
    public int RawDataRetentionDays { get; set; } = 7;
    public int HourlyAggregateRetentionDays { get; set; } = 30;
    public int DailyAggregateRetentionDays { get; set; } = 365;
    public int CleanupIntervalHours { get; set; } = 1;
}
