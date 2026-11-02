using Domain.Entities;
using Application.DTO.Employees;
using Application.DTO.Departments;
namespace Application.Mappers;

public static  partial class EmployeeMappingExtensions
{ 
    public static EmployeeByDepartmentResponseDto EmployeeToDepartmentResponseDto(this Employee e, string role = "")
    {
        if (e == null) throw new ArgumentNullException(nameof(e));
        return new EmployeeByDepartmentResponseDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            PhoneNumber = e.PhoneNumber,
            Position = e.Position,
            Gender = e.Gender,
            Role = role,
            IsActive = e.IsActive,
            JoinedDate = e.JoinedDate
        };
    }
}
