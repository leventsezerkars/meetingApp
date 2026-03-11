using Microsoft.EntityFrameworkCore;
using ToplantiApp.Domain.Entities;
using ToplantiApp.Domain.Enums;
using ToplantiApp.Domain.Interfaces;
using ToplantiApp.Infrastructure.Data;

namespace ToplantiApp.Infrastructure.Repositories;

public class MeetingRepository : GenericRepository<Meeting>, IMeetingRepository
{
    public MeetingRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Meeting?> GetByIdWithDetailsAsync(int id)
        => await Query
            .Include(m => m.CreatedBy)
            .Include(m => m.Participants).ThenInclude(p => p.User)
            .Include(m => m.Documents)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task<Meeting?> GetByAccessTokenAsync(Guid accessToken)
        => await Query
            .Include(m => m.CreatedBy)
            .Include(m => m.Participants)
            .Include(m => m.Documents)
            .FirstOrDefaultAsync(m => m.AccessToken == accessToken);

    public async Task<List<Meeting>> GetByUserIdAsync(int userId)
        => await Query
            .Include(m => m.CreatedBy)
            .Include(m => m.Participants)
            .Where(m => m.CreatedByUserId == userId
                     || m.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(m => m.StartDate)
            .ToListAsync();

    public async Task<List<Meeting>> GetCancelledMeetingsOlderThanAsync(DateTime date)
        => await Query
            .Include(m => m.Documents)
            .Include(m => m.Participants)
            .Where(m => m.Status == MeetingStatus.Cancelled && m.CancelledAt.HasValue && m.CancelledAt.Value < date)
            .ToListAsync();
}
