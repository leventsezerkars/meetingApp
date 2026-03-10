using System.Linq.Expressions;
using ToplantiApp.Domain.Entities;

namespace ToplantiApp.Domain.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    IQueryable<T> Query { get; }
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> ExistsAsync(int id);
}
