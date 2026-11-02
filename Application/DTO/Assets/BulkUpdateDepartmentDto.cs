namespace Application.DTO.Assets;
public class BulkUpdateDepartmentDto
{
    public List<Guid> AssetIds { get; set; } = new();
    public Guid NewDepartmentId { get; set; }
}