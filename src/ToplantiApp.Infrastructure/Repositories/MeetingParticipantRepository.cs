using Microsoft.EntityFrameworkCore;
using ToplantiApp.Domain.Entities;
using ToplantiApp.Domain.Interfaces;
using ToplantiApp.Infrastructure.Data;

namespace ToplantiApp.Infrastructure.Repositories;

public class MeetingParticipantRepository : GenericRepository<MeetingParticipant>, IMeetingParticipantRepository
{
    public MeetingParticipantRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<MeetingParticipant>> GetByMeetingIdAsync(int meetingId)
        => await Query
            .Include(mp => mp.User)
            .Where(mp => mp.MeetingId == meetingId)
            .ToListAsync();

    public async Task<bool> IsParticipantAsync(int meetingId, string email)
        => await Query.AnyAsync(mp => mp.MeetingId == meetingId && mp.Email == email);
}
