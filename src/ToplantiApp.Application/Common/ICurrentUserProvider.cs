namespace ToplantiApp.Application.Common;

/// <summary>
/// Mevcut HTTP isteğindeki kimliği doğrulanmış kullanıcı id'sini sağlar.
/// Audit interceptor tarafından CreatedByUserId / UpdatedByUserId atanması için kullanılır.
/// </summary>
public interface ICurrentUserProvider
{
    int? GetCurrentUserId();
}
