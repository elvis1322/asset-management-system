namespace Domain.Entities;
using System.Text.Json.Serialization;
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AssetStatus
{
    InStock = 1,
    Assigned = 2,
    InRepair = 3,
    Retired = 4
}