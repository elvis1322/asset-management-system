using Application.DTO.Departments;
using Domain.Entities;
namespace Application.Mappers;

public static partial class DepartmentMappingExtensions
{

    public static CachedDepartmentDto ToCachedDto(this Department d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        Code = d.Code,
        Description = d.Description
    };
}