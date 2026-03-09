using ToplantiApp.Domain.Enums;

namespace ToplantiApp.Domain.Entities;

public class Meeting : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public MeetingStatus Status { get; set; } = MeetingStatus.Active;
    public DateTime? CancelledAt { get; set; }
    public Guid AccessToken { get; set; } = Guid.NewGuid();

    public int CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public ICollection<MeetingParticipant> Participants { get; set; } = new List<MeetingParticipant>();
    public ICollection<MeetingDocument> Documents { get; set; } = new List<MeetingDocument>();
}
