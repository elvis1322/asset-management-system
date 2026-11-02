namespace Application.DTO.Assets;
public class AssetByEmployeeResponseDto
{
    public Guid Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AssignedDate { get; set; }
    public string? AssignmentNotes { get; set; }
}