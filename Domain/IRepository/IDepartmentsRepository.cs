using Domain.Entities;
namespace Domain.IRepository;

public interface IDepartmentsRepository
{
    Task<Department?> GetByIdTrackingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Department?> GetByIdNoTrackingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<Department> Items, int TotalCount)> GetPagedDepartmentsAsync(string? term, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<Department>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Department?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(Department department, CancellationToken cancellationToken = default);
    Task UpdateAsync(Department department, CancellationToken cancellationToken = default);
    Task DeleteAsync(Department department, CancellationToken cancellationToken = default);
    Task<bool> IsEmployeeInDepartmentAsync(Guid departmentId, Guid employeeId, CancellationToken cancellationToken);
    Task<bool> IsAssetInDepartmentAsync(Guid departmentId, Guid assetId, CancellationToken cancellationToken);
    Task<Department?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}