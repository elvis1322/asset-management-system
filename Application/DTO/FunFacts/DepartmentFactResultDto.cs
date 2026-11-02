namespace Application.DTO;
public class DepartmentFactResultDto
{
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string Fact { get; set; } = string.Empty;
}