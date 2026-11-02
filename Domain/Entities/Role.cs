using Microsoft.AspNetCore.Identity;
using Domain.Entities.Base;
namespace Domain.Entities;
public class Role : IdentityRole<Guid>, IAuditableEntity
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}