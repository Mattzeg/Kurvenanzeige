namespace Kurvenanzeige.Infrastructure.Data.Entities;

public class AnalogReading
{
    public int Id { get; set; }
    public string TagName { get; set; } = string.Empty;
    public int DbNumber { get; set; }
    public int Offset { get; set; }
    public float Value { get; set; }
    public string? Unit { get; set; }
    public int Quality { get; set; }
    public DateTime Timestamp { get; set; }
}
