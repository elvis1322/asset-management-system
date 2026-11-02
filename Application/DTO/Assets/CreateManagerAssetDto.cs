namespace Application.DTO.Assets;
public class CreateManagerAssetDto
{
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public decimal PurchaseValue { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public DateTime? WarrantyExpirationDate { get; set; }
    public bool IsShared { get; set; } = false;
}