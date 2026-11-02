using Application.DTO.Dashboard;
using Domain.Entities.Models.Dashboard;
namespace Application.Mappers;

public static partial class DashboardMappingExtensions
{
    public static CategoryCountDto ToDto(this CategoryCountModel m) => new()
    {
        Category = m.Category ?? "Pa Kategori",
        Count = m.Count
    };

}