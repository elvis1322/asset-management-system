namespace Application.DTO.Assets;
public class AssignedEmployeeDto
{
    public Guid EmployeeId { get; set; }
    public string FullName { get; set; } = string.Empty;
}