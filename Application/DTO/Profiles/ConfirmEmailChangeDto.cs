namespace Application.DTO.Profile;
public class ConfirmEmailChangeDto
{
    public string NewEmail { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}