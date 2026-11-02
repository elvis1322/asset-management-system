using System.ComponentModel.DataAnnotations;
namespace Application.DTO.Auth;
public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
