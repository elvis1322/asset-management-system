using Microsoft.AspNetCore.Identity;
using Domain.Entities.Base;
namespace Domain.Entities;
public class UserRole : IdentityUserRole<Guid>, IAuditableEntity
{
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}