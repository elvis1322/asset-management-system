namespace Application.DTO.Employees;
public class BulkUpdateEDepartmentDto
{
    public List<Guid> EmployeeIds { get; set; } = new();
    public Guid NewDepartmentId { get; set; }
}