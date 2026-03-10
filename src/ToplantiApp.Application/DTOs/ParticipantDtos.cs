namespace ToplantiApp.Application.DTOs;

public record AddParticipantDto(
    int? UserId,
    string? Email,
    string? FullName);

public record ParticipantDto
{
    public int Id { get; init; }
    public int? UserId { get; init; }
    public string Email { get; init; } = default!;
    public string FullName { get; init; } = default!;
    public string ParticipantType { get; init; } = default!;
    public DateTime InvitedAt { get; init; }
}

public record MeetingDocumentDto
{
    public int Id { get; init; }
    public string OriginalFileName { get; init; } = default!;
    public string ContentType { get; init; } = default!;
    public long FileSize { get; init; }
    public bool IsCompressed { get; init; }
    public DateTime UploadedAt { get; init; }
}
