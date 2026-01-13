namespace Kurvenanzeige.Core.Models;

public abstract class PlcDataPoint
{
    public string TagName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int DbNumber { get; set; }
    public int Offset { get; set; }
    public Quality Quality { get; set; } = Quality.Good;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public abstract DataPointType DataType { get; }
}
