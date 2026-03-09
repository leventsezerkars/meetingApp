namespace ToplantiApp.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? ProfileImagePath { get; set; }

    public ICollection<Meeting> CreatedMeetings { get; set; } = new List<Meeting>();
    public ICollection<MeetingParticipant> Participations { get; set; } = new List<MeetingParticipant>();

    public string FullName => $"{FirstName} {LastName}";
}
