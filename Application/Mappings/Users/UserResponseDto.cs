using Domain.Entities;
using Application.DTO.Auth;
namespace Application.Mappers;

public static partial class UserMappingExtensions
{
    public static UserResponseDto MapToDto(this User user, IList<string> roles)
    {
        if (user == null) return null;
        return new UserResponseDto
        {
            Id = user.Id,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Email = user.Email!,
            Gender = user.Gender,
            Roles = roles?.ToList() ?? new List<string>()
        };
    }
}