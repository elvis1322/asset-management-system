using Application.DTO.Assets;
using Application.Models;
using Domain.Entities;
using ErrorOr;
namespace Application.Interfaces;

public interface IAssetsService
{
    Task<ErrorOr<PagedResult<AssetByEmployeeResponseDto>>> GetAssetsByEmployeeAsync(Guid employeeId, PaginationParams paginationParams, CancellationToken cancellationToken = default);
    Task<ErrorOr<PagedResult<AssetResponseDto>>> GetAssetsPagedAsync(PaginationParams paginationParams, string? term, AssetStatus? status, AssetCategory? category, CancellationToken cancellationToken = default);
    Task<ErrorOr<PagedResult<AssetByDepartmentResponseDto>>> GetAssetsByDepartmentAsync(Guid departmentId, PaginationParams paginationParams, AssetStatus? status, AssetCategory? category, CancellationToken cancellationToken = default);
    Task<ErrorOr<PagedResult<AssetAssignmentHistoryDto>>> GetHistoryAsync(Guid assetId, PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<ErrorOr<AssetResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ErrorOr<List<AssetResponseDto>>> GetMyAssetsAsync(CancellationToken cancellationToken = default);
    Task<ErrorOr<AssetResponseDto>> CreateAsync(CreateAssetDto dto, CancellationToken cancellationToken = default);
    Task<ErrorOr<AssetResponseDto>> UpdateAsync(Guid id, UpdateAssetDto dto, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> AssignAssetAsync(Guid assetId, AssignAssetDto dto, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> UnassignAssetAsync(Guid assetId, UnassignAssetDto dto, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> UpdateDepartmentAsync(Guid assetId, Guid newDepartmentId, CancellationToken cancellationToken = default);
    Task<ErrorOr<AssetResponseDto>> CreateAssetByManagerAsync(CreateManagerAssetDto dto, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> UpdateDepartmentMultipleAsync(List<Guid> assetIds, Guid newDepartmentId, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> DeleteMultipleAsync(List<Guid> ids, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> UnassignAssetsForEmployeesAsync(List<Guid> employeeIds, CancellationToken cancellationToken = default);
}