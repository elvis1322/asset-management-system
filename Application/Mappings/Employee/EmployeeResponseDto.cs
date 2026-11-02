using Domain.Entities;
using Application.DTO.Employees;
using Application.DTO.Departments;
namespace Application.Mappers;

public static  partial class EmployeeMappingExtensions
{

    public static EmployeeResponseDto EmployeeResponseDto(this Employee e, string role = "")
    {
        if (e == null) throw new ArgumentNullException(nameof(e));
        return new EmployeeResponseDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            PhoneNumber = e.PhoneNumber,
            Position = e.Position,
            Gender = e.Gender,
            Department = new DepartmentDto
            {
                 Id=e.Department.Id,
                Name = e.Department.Name,
                Code = e.Department.Code
            },
            Role = role,
            IsActive = e.IsActive,
            JoinedDate = e.JoinedDate
        };
    }
}