namespace ToplantiApp.Domain.Entities;

public class MeetingLog
{
    public int Id { get; set; }
    public int MeetingId { get; set; }
    public string MeetingName { get; set; } = string.Empty;
    public bool DeletedBySystem { get; set; }
    public DateTime DeletedAt { get; set; }
    public string? LogData { get; set; }
}
