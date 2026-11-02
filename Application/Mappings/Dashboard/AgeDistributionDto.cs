using Application.DTO.Dashboard;
using Domain.Entities.Models.Dashboard;
namespace Application.Mappers;

public static partial class DashboardMappingExtensions
{
    public static AgeDistributionDto ToDto(this AgeDistributionModel m) => new()
    {
        AgeRange = m.AgeRange,
        Count = m.Count
    };
}