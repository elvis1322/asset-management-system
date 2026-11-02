using Domain.Entities.Base;
namespace Domain.Entities;
public class AssetAssignment : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AssetId { get; set; }
    public Asset Asset { get; set; } = null!;
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ReturnedDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}