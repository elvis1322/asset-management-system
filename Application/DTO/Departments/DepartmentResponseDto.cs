namespace Application.DTO.Departments;
public class DepartmentResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AssetCount { get; set; }
    public int EmployeeCount { get; set; }
}