using Domain.Entities;
namespace Application.Interfaces;

public interface ITokenService
{
    string CreateToken(User user);
    string GenerateJwtToken(User user, IList<string> roles);
    string GenerateRefreshToken();
}