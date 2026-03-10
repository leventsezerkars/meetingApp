using ToplantiApp.Domain.Entities;

namespace ToplantiApp.Domain.Interfaces;

public interface IMeetingParticipantRepository : IGenericRepository<MeetingParticipant>
{
    Task<List<MeetingParticipant>> GetByMeetingIdAsync(int meetingId);
    Task<bool> IsParticipantAsync(int meetingId, string email);
}
