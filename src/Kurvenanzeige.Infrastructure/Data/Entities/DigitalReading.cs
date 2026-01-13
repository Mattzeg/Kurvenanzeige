namespace Kurvenanzeige.Infrastructure.Data.Entities;

public class DigitalReading
{
    public int Id { get; set; }
    public string TagName { get; set; } = string.Empty;
    public int DbNumber { get; set; }
    public int Offset { get; set; }
    public int Bit { get; set; }
    public bool Value { get; set; }
    public int Quality { get; set; }
    public DateTime Timestamp { get; set; }
}
