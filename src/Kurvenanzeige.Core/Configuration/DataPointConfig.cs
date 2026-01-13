using Kurvenanzeige.Core.Models;

namespace Kurvenanzeige.Core.Configuration;

public class DataPointConfig
{
    public string TagName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DataPointType DataType { get; set; }
    public int DbNumber { get; set; }
    public int Offset { get; set; }
    public int? Bit { get; set; }
    public string? Unit { get; set; }
    public float? MinValue { get; set; }
    public float? MaxValue { get; set; }
    public bool IsEnabled { get; set; } = true;
    public int PollingInterval { get; set; } = 5000;
}
