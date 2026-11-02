namespace Application.DTO.Dashboard;
public class FullDashboardDto
{
    public decimal TotalPurchaseValue { get; set; }
    public List<DepartmentValueDto> ValueByDepartment { get; set; } = new();
    public List<StatusBreakdownDto> StatusBreakdown { get; set; } = new();
    public List<CategoryCountDto> AssetsByCategory { get; set; } = new();
    public List<AgeDistributionDto> AgeDistribution { get; set; } = new();
}