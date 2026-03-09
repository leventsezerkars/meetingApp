using ToplantiApp.Domain.Entities;

namespace ToplantiApp.Domain.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
