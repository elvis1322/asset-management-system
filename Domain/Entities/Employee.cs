using Domain.Entities.Base;
namespace Domain.Entities;
public class Employee : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Position { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
    public string Gender { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public Guid DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public ICollection<AssetAssignment> Assignments { get; set; } = new List<AssetAssignment>();
}