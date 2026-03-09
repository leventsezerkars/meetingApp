using ToplantiApp.Domain.Entities;

namespace ToplantiApp.Domain.Interfaces;

public interface IMeetingRepository : IGenericRepository<Meeting>
{
    Task<Meeting?> GetByIdWithDetailsAsync(int id);
    Task<Meeting?> GetByAccessTokenAsync(Guid accessToken);
    Task<IReadOnlyList<Meeting>> GetByUserIdAsync(int userId);
    Task<IReadOnlyList<Meeting>> GetCancelledMeetingsOlderThanAsync(DateTime date);
}
