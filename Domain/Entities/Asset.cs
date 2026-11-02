using Domain.Entities.Base;
namespace Domain.Entities;
public class Asset : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public AssetStatus Status { get; set; } = AssetStatus.InStock;
    public AssetCategory Category { get; set; }
    public decimal PurchaseValue { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime? WarrantyExpirationDate { get; set; }
    public bool IsShared { get; set; } = false;
    public Guid DepartmentId { get; set; }
    public  Department? Department { get; set; } 
    public ICollection<AssetAssignment> Assignments { get; set; } = new List<AssetAssignment>();
}