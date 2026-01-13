namespace Kurvenanzeige.Infrastructure.Data.Entities;

public class DataBlockReading
{
    public int Id { get; set; }
    public string TagName { get; set; } = string.Empty;
    public int DbNumber { get; set; }
    public string StructureJson { get; set; } = string.Empty;
    public int Quality { get; set; }
    public DateTime Timestamp { get; set; }
}
