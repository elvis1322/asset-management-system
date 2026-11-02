namespace Application.DTO.Auth;
public class UserResponseLoginDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; } 
    public required string FullName { get; set; } 
     public required string Gender { get; set; }
    public List<string> Roles { get; set; } = new();
    public string? AccessToken { get; set; }
    public string? RefreshToken{get; set;}
}