using Application.DTO.Dashboard;
using Domain.Entities.Models.Dashboard;
namespace Application.Mappers;

public static partial class DashboardMappingExtensions
{
    public static StatusBreakdownDto ToDto(this StatusBreakdownModel m) => new()
    {
        Status = m.Status,
        Count = m.Count
    };
}