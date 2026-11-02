namespace Application.DTO.Assets;
public class AssetAssignmentHistoryDto
{
    public Guid Id { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
}