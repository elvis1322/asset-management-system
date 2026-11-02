using Domain.Entities;
using Application.DTO.Auth;
namespace Application.Mappers;

public static partial class UserMappingExtensions
{
    public static UserResponseLoginDto ToResponseLoginDto(
         this User user,
         IList<string> roles,
         string? newAccessToken,
         string? newRefreshTokenValue
         )
    {
        if (user == null) return null;
        return new UserResponseLoginDto
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Gender = user.Gender,
            Roles = roles?.ToList() ?? new List<string>(),
            AccessToken = newAccessToken ?? string.Empty,
            RefreshToken = newRefreshTokenValue ?? string.Empty
        };
    }
}