namespace ToplantiApp.Domain.Entities;

public class MeetingDocument : BaseEntity
{
    public int MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public bool IsCompressed { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
