using Domain.Entities;
using Application.DTO.Assets;
namespace Application.Mappers;
public static  partial class AssetMappingExtensions
{
    public static AssetByDepartmentResponseDto AssetToDepartmentResponseDto(this Asset a)
    {
        var activeAssignments = a.Assignments?
            .Where(aa => aa.IsActive)
            .ToList() ?? new List<AssetAssignment>();

        return new AssetByDepartmentResponseDto
        {
            Id = a.Id,
            AssetTag = a.AssetTag,
            Name = a.Name,
            Category = a.Category.ToString(),
            SerialNumber = a.SerialNumber,
            Status = a.Status.ToString(),
            PurchaseValue = a.PurchaseValue,
            PurchaseDate = a.PurchaseDate,
            IsShared = a.IsShared,
            AssignedEmployees = activeAssignments
                .Where(aa => aa.Employee != null)
                .Select(aa => new AssignedEmployeeDto
                {
                    EmployeeId = aa.EmployeeId,
                    FullName = $"{aa.Employee.FirstName} {aa.Employee.LastName}"
                })
                .ToList()
        };
    }
}