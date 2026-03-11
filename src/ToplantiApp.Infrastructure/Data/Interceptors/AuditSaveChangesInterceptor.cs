using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ToplantiApp.Application.Common;
using ToplantiApp.Domain.Entities;

namespace ToplantiApp.Infrastructure.Data.Interceptors;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserProvider _currentUserProvider;

    public AuditSaveChangesInterceptor(ICurrentUserProvider currentUserProvider)
    {
        _currentUserProvider = currentUserProvider;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            UpdateAuditFields(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
            UpdateAuditFields(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    private void UpdateAuditFields(DbContext context)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var entries = context.ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                if (userId.HasValue)
                    entry.Entity.CreatedByUserId = userId;
            }
            else if (entry.State == EntityState.Modified && entry.Entity is AuditableEntity auditable)
            {
                auditable.UpdatedAt = DateTime.UtcNow;
                if (userId.HasValue)
                    auditable.UpdatedByUserId = userId;
            }
        }
    }
}
