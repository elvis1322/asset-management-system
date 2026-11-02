namespace Application.DTO.Assets;
public class AssetByDepartmentResponseDto
{
    public Guid Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal PurchaseValue { get; set; }
    public DateTime PurchaseDate { get; set; }
    public bool IsShared { get; set; }
    public List<AssignedEmployeeDto> AssignedEmployees { get; set; } = new();
}