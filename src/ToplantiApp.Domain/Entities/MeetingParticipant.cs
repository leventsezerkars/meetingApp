using ToplantiApp.Domain.Enums;

namespace ToplantiApp.Domain.Entities;

public class MeetingParticipant : BaseEntity
{
    public int MeetingId { get; set; }
    public Meeting Meeting { get; set; } = null!;

    public int? UserId { get; set; }
    public User? User { get; set; }

    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public ParticipantType ParticipantType { get; set; }
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
}
