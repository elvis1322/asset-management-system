using Application.DTO.Departments;
using ErrorOr;
using Domain.Entities;
using Application.Models;
namespace Application.Interfaces;

public interface IDepartmentsService
{
  Task<Department?> GetByIdTrackingAsync(Guid id, CancellationToken cancellationToken = default);
  Task<bool> IsEmployeeInDepartmentAsync(Guid departmentId, Guid employeeId, CancellationToken cancellationToken);
  Task<bool> IsAssetInDepartmentAsync(Guid departmentId, Guid assetId, CancellationToken cancellationToken);
  Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
  Task<ErrorOr<PagedResult<DepartmentResponseDto>>> GetPagedAsync(  PaginationParams paginationParams,string? term, CancellationToken cancellationToken = default);
  Task<ErrorOr<DepartmentResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
  Task<ErrorOr<DepartmentResponseDto>> CreateAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default);
  Task<ErrorOr<DepartmentResponseDto>> UpdateByManagerAsync(UpdateDepartmentDto dto, CancellationToken cancellationToken = default);
  Task<ErrorOr<DepartmentResponseDto>> UpdateAsync(Guid id, UpdateDepartmentDto dto, CancellationToken cancellationToken = default);
  Task<ErrorOr<Success>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
  Task UpdateDepartmentFunFactAsync(Guid departmentId, string funFact, CancellationToken cancellationToken = default);
  Task<ErrorOr<DepartmentResponseDto>> UpdateCodeAsync(Guid id, UpdateDepartamentCodeDto dto, CancellationToken cancellationToken = default);
}
