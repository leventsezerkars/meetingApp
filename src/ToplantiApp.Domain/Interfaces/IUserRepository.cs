using ToplantiApp.Domain.Entities;

namespace ToplantiApp.Domain.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<IReadOnlyList<User>> SearchUsersAsync(string searchTerm);
}
