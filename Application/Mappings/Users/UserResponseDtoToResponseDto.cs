using Domain.Entities;
using Application.DTO.Auth;
namespace Application.Mappers;

public static partial class UserMappingExtensions
{
    public static UserResponseDto ToResponseDto(
        this User user,
        IList<string> roles
    )
    {
        if (user == null) return null;
        return new UserResponseDto
        {
            Id = user.Id,
            Email = user.Email!,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Gender = user.Gender,
            Roles = roles?.ToList() ?? new List<string>()
        };
    }
}