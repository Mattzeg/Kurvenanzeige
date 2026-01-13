namespace Kurvenanzeige.Core.Models;

public class StringValue : PlcDataPoint
{
    public override DataPointType DataType => DataPointType.String;
    public string Value { get; set; } = string.Empty;
    public int MaxLength { get; set; } = 254; // S7 String default max length
}
