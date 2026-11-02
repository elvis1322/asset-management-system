using Domain.Entities.Base;
namespace Domain.Entities;
public class RefreshToken : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public User User { get; set; } = null!;
    public string UserToken { get; set; } = string.Empty;
    public Guid UserId { get; set; } 
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public bool IsRevoked { get; set; } = false;
}