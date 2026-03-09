namespace ToplantiApp.Application.DTOs;

public record CreateMeetingDto(
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate);

public record UpdateMeetingDto(
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate);

public record MeetingDto(
    int Id,
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    DateTime? CancelledAt,
    Guid AccessToken,
    string MeetingUrl,
    UserDto CreatedBy,
    List<ParticipantDto> Participants,
    List<MeetingDocumentDto> Documents,
    DateTime CreatedAt);

public record MeetingListDto(
    int Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    int ParticipantCount,
    string CreatedByName,
    DateTime CreatedAt);

public record MeetingRoomDto(
    int Id,
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime EndDate,
    string Status,
    string CreatedByName,
    List<ParticipantDto> Participants,
    List<MeetingDocumentDto> Documents);

public record MeetingAccessResultDto(
    bool IsAccessible,
    string? Message,
    MeetingRoomDto? Meeting);
