namespace Kurvenanzeige.Core.Models;

public class DataBlockValue : PlcDataPoint
{
    public override DataPointType DataType => DataPointType.DataBlock;
    public string StructureJson { get; set; } = string.Empty;
    public byte[] RawData { get; set; } = Array.Empty<byte>();
}
