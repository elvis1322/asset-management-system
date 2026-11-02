using Application.DTO.Departments;
namespace Application.DTO.Assets;
public class AssetResponseDto
{
    public Guid Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal PurchaseValue { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime? WarrantyExpirationDate { get; set; }
    public DepartmentDto Department { get; set; }
    public List<AssignedEmployeeDto> AssignedEmployees { get; set; } = new();
    public bool IsShared { get; set; }
}


