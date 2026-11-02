namespace Application.DTO.Auth;
public class LogoutRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
    public bool RevokeAllDevices { get; set; } = false;
}