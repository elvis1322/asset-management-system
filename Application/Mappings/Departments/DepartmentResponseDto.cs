using Application.DTO.Departments;
using Domain.Entities;
namespace Application.Mappers;

public static partial class DepartmentMappingExtensions
{
     public static DepartmentResponseDto ToDto(this Department d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        Code = d.Code,
        Description = d.Description,
        EmployeeCount = d.EmployeeCount,
        AssetCount = d.AssetCount
    };
}