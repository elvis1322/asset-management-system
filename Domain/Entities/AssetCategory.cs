namespace Domain.Entities;
using System.Text.Json.Serialization;
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AssetCategory
{
    Laptop = 1,
    Monitor = 2,
    Printer = 3,
    Phone = 4,
    Furniture = 5 
}