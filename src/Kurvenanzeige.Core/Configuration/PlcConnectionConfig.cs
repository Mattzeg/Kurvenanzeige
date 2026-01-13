namespace Kurvenanzeige.Core.Configuration;

public class PlcConnectionConfig
{
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 102;
    public int Rack { get; set; } = 0;
    public int Slot { get; set; } = 1;
    public string CpuType { get; set; } = "S71500";
    public int ConnectTimeout { get; set; } = 5000;
    public int ReadTimeout { get; set; } = 2000;
    public int ReconnectDelay { get; set; } = 5000;
    public int MaxReconnectAttempts { get; set; } = 10;
}
