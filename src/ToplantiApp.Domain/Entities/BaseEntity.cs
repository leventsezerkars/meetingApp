namespace ToplantiApp.Domain.Entities;

/// <summary>
/// Sadece Create audit (CreatedAt, CreatedByUserId). MeetingParticipant, MeetingDocument gibi güncelleme takibi gerekmeyen entity'ler için.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
}
