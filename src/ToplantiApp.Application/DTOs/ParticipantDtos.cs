namespace ToplantiApp.Application.DTOs;

public record AddParticipantDto(
    int? UserId,
    string? Email,
    string? FullName);

public record ParticipantDto(
    int Id,
    int? UserId,
    string Email,
    string FullName,
    string ParticipantType,
    DateTime InvitedAt);

public record MeetingDocumentDto(
    int Id,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    bool IsCompressed,
    DateTime UploadedAt);
