using Domain.Entities.Models.Dashboard;
namespace Domain.IRepository;

public interface IDashboardRepository
{
    Task<decimal> GetTotalPurchaseValueAsync(Guid? departmentId = null, CancellationToken cancellationToken = default);
    Task<List<DepartmentValueModel>> GetValueByDepartmentAsync(Guid? departmentId = null, CancellationToken cancellationToken = default);
    Task<List<StatusBreakdownModel>> GetStatusBreakdownAsync(Guid? departmentId = null, CancellationToken cancellationToken = default);
    Task<List<CategoryCountModel>> GetAssetsByCategoryAsync(Guid? departmentId = null, CancellationToken cancellationToken = default);
    Task<List<AgeDistributionModel>> GetAgeDistributionAsync(Guid? departmentId = null, CancellationToken cancellationToken = default);
}