using Application.DTO.Employees;
using ErrorOr;
using Application.Models;
namespace Application.Interfaces;

public interface IEmployeeService
{
    Task<ErrorOr<EmployeeResponseDto>> GetEmployeeByIdAsync(Guid employeeId, CancellationToken cancellationToken = default);
    Task<ErrorOr<PagedResult<EmployeeResponseDto>>> GetEmployeesPagedAsync(PaginationParams paginationParams, string? term, CancellationToken cancellationToken = default);
    Task<ErrorOr<PagedResult<EmployeeByDepartmentResponseDto>>> GetEmployeesByDepartmentAsync(Guid departmentId, PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<ErrorOr<EmployeeResponseDto>> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default);
    Task<ErrorOr<EmployeeResponseDto>> UpdateAsync(Guid id, UpdateEmployeeDto dto, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ErrorOr<EmployeeResponseDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> UpdateDepartmentAsync(Guid employeeId, Guid newDepartmentId, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> DeleteMultipleAsync(List<Guid> employeeIds, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> UpdateDepartmentMultipleAsync(List<Guid> employeeIds, Guid newDepartmentId, CancellationToken cancellationToken = default);
    Task<ErrorOr<EmployeeResponseDto>> PromoteUserToEmployeeAsync(Guid userId, PromoteUserDto dto, CancellationToken cancellationToken = default);
    Task<ErrorOr<Updated>> UpdateUserRoleAsync(Guid employeeId, UpdateUserRoleDto dto, CancellationToken cancellationToken = default);
}