using Domain.Entities;
using Application.DTO.Assets;
using Application.DTO.Departments;
namespace Application.Mappers;

public static  partial class AssetMappingExtensions
{

    public static AssetResponseDto AssetResponseDto(this Asset a)
    {
        var activeAssignments = a.Assignments?
            .Where(aa => aa.IsActive)
            .ToList() ?? new List<AssetAssignment>();

        return new AssetResponseDto
        {
            Id = a.Id,
            AssetTag = a.AssetTag,
            Name = a.Name,
            Category = a.Category.ToString(),
            SerialNumber = a.SerialNumber,
            Status = a.Status.ToString(),
            PurchaseValue = a.PurchaseValue,
            PurchaseDate = a.PurchaseDate,
            WarrantyExpirationDate = a.WarrantyExpirationDate,
            Department = a.Department != null ? new DepartmentDto
            {
                Id=a.Department.Id,
                Name = a.Department.Name,
                Code = a.Department.Code
            } : null!,
            AssignedEmployees = activeAssignments
                .Where(aa => aa.Employee != null)
                .Select(aa => new AssignedEmployeeDto
                {
                    EmployeeId = aa.EmployeeId,
                    FullName = $"{aa.Employee.FirstName} {aa.Employee.LastName}"
                })
                .ToList(),
            IsShared = a.IsShared
        };
    }
}
