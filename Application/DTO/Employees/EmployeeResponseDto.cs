using Application.DTO.Departments;
namespace Application.DTO.Employees;
public class EmployeeResponseDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DepartmentDto? Department { get; set; } 
    public string Role { get; set; } = string.Empty; 
    public bool IsActive { get; set; }
    public DateTime JoinedDate { get; set; }
    public string Gender { get; set; } = string.Empty;
}
