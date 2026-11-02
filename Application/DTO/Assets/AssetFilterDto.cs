using Domain.Entities;
namespace Application.DTO.Assets;
public class AssetFilterDto
{
    public string? SearchTerm { get; set; }
    public AssetStatus? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}