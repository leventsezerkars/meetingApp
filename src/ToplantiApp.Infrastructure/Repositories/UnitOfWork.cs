using ToplantiApp.Domain.Interfaces;
using ToplantiApp.Infrastructure.Data;

namespace ToplantiApp.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IUserRepository? _users;
    private IMeetingRepository? _meetings;
    private IMeetingParticipantRepository? _meetingParticipants;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IMeetingRepository Meetings => _meetings ??= new MeetingRepository(_context);
    public IMeetingParticipantRepository MeetingParticipants =>
        _meetingParticipants ??= new MeetingParticipantRepository(_context);

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
