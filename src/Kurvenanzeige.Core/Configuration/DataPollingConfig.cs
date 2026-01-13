namespace Kurvenanzeige.Core.Configuration;

public class DataPollingConfig
{
    public int PollingIntervalMs { get; set; } = 5000;
    public int BatchSize { get; set; } = 50;
    public bool EnableBuffering { get; set; } = true;
    public int BufferFlushIntervalMs { get; set; } = 1000;
}
