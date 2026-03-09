namespace ToplantiApp.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IMeetingRepository Meetings { get; }
    IMeetingParticipantRepository MeetingParticipants { get; }
    Task<int> SaveChangesAsync();
}
