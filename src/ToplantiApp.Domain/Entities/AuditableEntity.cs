namespace ToplantiApp.Domain.Entities;

/// <summary>
/// Create + Update audit. Meeting, User gibi hem oluşturan hem güncelleyen bilgisinin tutulduğu entity'ler için.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTime? UpdatedAt { get; set; }
    public int? UpdatedByUserId { get; set; }
}
