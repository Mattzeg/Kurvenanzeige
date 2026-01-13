namespace Kurvenanzeige.Infrastructure.Data.Entities;

public class StringReading
{
    public int Id { get; set; }
    public string TagName { get; set; } = string.Empty;
    public int DbNumber { get; set; }
    public int Offset { get; set; }
    public string Value { get; set; } = string.Empty;
    public int MaxLength { get; set; }
    public int Quality { get; set; }
    public DateTime Timestamp { get; set; }
}
