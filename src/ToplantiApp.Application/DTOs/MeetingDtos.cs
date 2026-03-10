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

public record MeetingDto
{
    public int Id { get; init; }
    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Status { get; init; } = default!;
    public DateTime? CancelledAt { get; init; }
    public Guid AccessToken { get; init; }
    public string MeetingUrl { get; init; } = default!;
    public UserDto CreatedBy { get; init; } = default!;
    public List<ParticipantDto> Participants { get; init; } = [];
    public List<MeetingDocumentDto> Documents { get; init; } = [];
    public DateTime CreatedAt { get; init; }
}

public record MeetingListDto
{
    public int Id { get; init; }
    public string Name { get; init; } = default!;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Status { get; init; } = default!;
    public int ParticipantCount { get; init; }
    public string CreatedByName { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
}

public record MeetingRoomDto
{
    public int Id { get; init; }
    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Status { get; init; } = default!;
    public string CreatedByName { get; init; } = default!;
    public List<ParticipantDto> Participants { get; init; } = [];
    public List<MeetingDocumentDto> Documents { get; init; } = [];
}

public record MeetingAccessResultDto
{
    public bool IsAccessible { get; init; }
    public string? Message { get; init; }
    public MeetingRoomDto? Meeting { get; init; }
}
