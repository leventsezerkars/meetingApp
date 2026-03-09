using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ToplantiApp.Domain.Entities;
using ToplantiApp.Domain.Interfaces;
using ToplantiApp.Infrastructure.Data;

namespace ToplantiApp.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
        => await _dbSet.FindAsync(id);

    public async Task<IReadOnlyList<T>> GetAllAsync()
        => await _dbSet.ToListAsync();

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
        => await _dbSet.Where(predicate).ToListAsync();

    public async Task<T> AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        return entity;
    }

    public void Update(T entity)
        => _dbSet.Update(entity);

    public void Delete(T entity)
        => _dbSet.Remove(entity);

    public async Task<bool> ExistsAsync(int id)
        => await _dbSet.AnyAsync(e => e.Id == id);
}
