using Domain.Entities;
namespace Domain.IRepository;

public interface IAssetsRepository
{
    Task<(List<Asset> Items, int TotalCount)> GetPagedAssetsAsync(string? term, AssetStatus? status, AssetCategory? category, Guid? departmentId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<(List<Asset> Items, int TotalCount)> GetPagedByEmployeeIdAsync(Guid employeeId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<(List<Asset> Items, int TotalCount)> GetPagedByDepartmentIdAsync(Guid departmentId, AssetStatus? status, AssetCategory? category, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<(List<AssetAssignment> Items, int TotalCount)> GetAssignmentHistoryPagedAsync(Guid assetId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Asset?> GetByTrackingIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Asset?> GetByIdNoTrackingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Asset>> GetByIdsWithTrackingAsync(List<Guid> ids, CancellationToken cancellationToken = default);
    Task<Asset?> GetByTagOrSerialAsync(string assetTag, string serialNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetAssetsByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<AssetAssignment?> GetActiveAssignmentAsync(Guid assetId, Guid employeeId, CancellationToken cancellationToken = default);
    Task AddAsync(Asset asset, CancellationToken cancellationToken = default);
    Task UpdateAsync(Asset asset, CancellationToken cancellationToken = default);
    Task DeleteAsync(Asset asset, CancellationToken cancellationToken = default);
    Task UpdateAssignmentAsync(AssetAssignment assignment, CancellationToken cancellationToken = default);
    Task AddAssignmentAsync(AssetAssignment assignment, CancellationToken cancellationToken = default);
    Task<int> DeleteMultipleAsync(List<Guid> assetIds, CancellationToken cancellationToken = default);
    Task<int> UpdateDepartmentAsync(List<Guid> assetIds, Guid newDepartmentId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> GetDepartmentCountsForAssetsAsync(List<Guid> assetIds, Guid newDepartmentId, CancellationToken cancellationToken = default);
    Task UnassignAssetsForEmployeesAsync(List<Guid> employeeIds, CancellationToken cancellationToken = default);
    Task<List<Asset>> GetAssetsExpiringInDaysAsync(int days, CancellationToken cancellationToken = default);
    Task<List<Asset>> GetExpiredAssetsAsync(CancellationToken cancellationToken = default);
}