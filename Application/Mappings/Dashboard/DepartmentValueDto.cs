using Application.DTO.Dashboard;
using Domain.Entities.Models.Dashboard;
namespace Application.Mappers;

public static partial  class DashboardMappingExtensions
{

    public static DepartmentValueDto ToDto(this DepartmentValueModel m) => new()
    {
        DepartmentName = m.DepartmentName,
        TotalValue = m.TotalValue,
        AssetCount = m.AssetCount,
        EmployeeCount = m.EmployeeCount
    };

}