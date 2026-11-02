using Domain.Entities;
using Application.DTO.Assets;
namespace Application.Mappers;

public static partial class AssetMappingExtensions
{

    public static AssetByEmployeeResponseDto AssetToEmployeeResponseDto(this Asset a)
    {
        var activeAssignment = a.Assignments?.FirstOrDefault(aa => aa.IsActive);

        return new AssetByEmployeeResponseDto
        {
            Id = a.Id,
            AssetTag = a.AssetTag,
            Name = a.Name,
            Category = a.Category.ToString(),
            Status = a.Status.ToString(),
            AssignedDate = activeAssignment?.AssignedDate ?? DateTime.MinValue,
            AssignmentNotes = activeAssignment?.Notes
        };
    }
}