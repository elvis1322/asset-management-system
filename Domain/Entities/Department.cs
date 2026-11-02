using Domain.Entities.Base;
namespace Domain.Entities;
public class Department : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public required string Code { get; set; }
    public string Description { get; set; } = string.Empty;
    public int AssetCount{get; set;}=0;
    public int EmployeeCount{get;set;}=0;
    public string? DailyFunFact { get; set; }
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}