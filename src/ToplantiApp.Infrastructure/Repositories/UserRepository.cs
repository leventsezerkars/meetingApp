using Microsoft.EntityFrameworkCore;
using ToplantiApp.Domain.Entities;
using ToplantiApp.Domain.Interfaces;
using ToplantiApp.Infrastructure.Data;

namespace ToplantiApp.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
        => await _dbSet.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<bool> EmailExistsAsync(string email)
        => await _dbSet.AnyAsync(u => u.Email == email);

    public async Task<IReadOnlyList<User>> SearchUsersAsync(string searchTerm)
    {
        var term = searchTerm.ToLower();
        return await _dbSet
            .Where(u => u.FirstName.ToLower().Contains(term)
                     || u.LastName.ToLower().Contains(term)
                     || u.Email.ToLower().Contains(term))
            .Take(20)
            .ToListAsync();
    }
}
