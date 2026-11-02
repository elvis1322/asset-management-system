using Domain.Entities;
using Application.DTO.Profile;
namespace Application.Mappers;

public static partial class UserMappingExtensions
{
    public static ProfileResponseDto MapToProfileResponseDto(this User user, IList<string> roles)
    {
        if (user == null) return null;
        return new ProfileResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            Gender = user.Gender,
            Roles = roles?.ToList() ?? new List<string>()
        };
    }

}