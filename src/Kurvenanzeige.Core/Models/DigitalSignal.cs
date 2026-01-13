namespace Kurvenanzeige.Core.Models;

public class DigitalSignal : PlcDataPoint
{
    public override DataPointType DataType => DataPointType.Digital;
    public bool Value { get; set; }
    public int Bit { get; set; }
}
