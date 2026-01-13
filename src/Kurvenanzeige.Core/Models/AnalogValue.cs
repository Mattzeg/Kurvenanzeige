namespace Kurvenanzeige.Core.Models;

public class AnalogValue : PlcDataPoint
{
    public override DataPointType DataType => DataPointType.Analog;
    public float Value { get; set; }
    public string? Unit { get; set; }
    public float? MinValue { get; set; }
    public float? MaxValue { get; set; }
}
