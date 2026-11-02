namespace Application.DTO.Assets;
public class UpdateAssetDto
{
  public string Name { get; set; } = string.Empty;
  public string Category { get; set; } = string.Empty;
  public decimal PurchaseValue { get; set; }
  public DateTime? WarrantyExpirationDate { get; set; }
  public bool IsShared { get; set; }
}