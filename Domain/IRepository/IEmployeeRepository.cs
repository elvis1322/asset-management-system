using Domain.Entities;
namespace Domain.IRepository;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdNoTrackingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<Employee> Items, int TotalCount)> GetPagedEmployeesAsync(string? term, Guid? departmentId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<(List<Employee> Items, int TotalCount)> GetPagedByDepartmentIdAsync(Guid departmentId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Employee?> GetByIdTrackingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Employee?> GetByUserIdNoTrackingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Employee?> GetByUserIdTrackingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Employee employee, CancellationToken cancellationToken = default);
    Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default);
    Task DeleteAsync(Employee employee, CancellationToken cancellationToken = default);
    Task<List<Employee>> GetByIdsWithTrackingAsync(List<Guid> employeeIds, CancellationToken cancellationToken = default);
    Task<int> DeleteMultipleAsync(List<Guid> employeeIds, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> GetDepartmentCountsForDeleteAsync(List<Guid> employeeIds, CancellationToken cancellationToken = default);
}