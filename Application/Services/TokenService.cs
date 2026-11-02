using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Helpers;
using Domain.Entities;
using Application.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
namespace Application.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly SymmetricSecurityKey _key;
    private readonly SigningCredentials _creds;
    public TokenService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        _creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
    }

    public string CreateToken(User user)
    {
        var roles = user.UserRoles?
            .Where(ur => ur.Role != null && !string.IsNullOrEmpty(ur.Role.Name))
            .Select(ur => ur.Role!.Name!)
            .ToList() ?? new List<string>();
        return GenerateJwtToken(user, roles);
    }

    public string GenerateJwtToken(User user, IList<string> roles)
    {
        var claims = GetBaseClaims(user);
        if (roles != null)
        {
            foreach (var role in roles)
            {
                if (!string.IsNullOrEmpty(role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
        }
        return BuildToken(claims);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
    private static List<Claim> GetBaseClaims(User user) => new()
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Email, user.Email ?? string.Empty),
        new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}".Trim()),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };
    private string BuildToken(List<Claim> claims)
    {
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes),
            SigningCredentials = _creds,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}