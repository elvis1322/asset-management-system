namespace Application.DTO.Departments;
public class DepartmentDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public required string Code { get; set; }
}