namespace Domain.Entities.Models.Dashboard;
public class DepartmentValueModel
{
    public string DepartmentName { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public int AssetCount { get; set; }
    public int EmployeeCount { get; set; }
}