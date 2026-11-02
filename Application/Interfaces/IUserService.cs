using ErrorOr;
using Application.DTO.Admin;
using Application.DTO.Auth;
using Application.Models;
namespace Application.Interfaces;

public interface IUserService
{
    Task<ErrorOr<PagedResult<UserResponseDto>>> GetUsersPagedAsync(PaginationParams paginationParams, string? term, CancellationToken cancellationToken = default);
    Task<ErrorOr<UserResponseDto>> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ErrorOr<UserResponseDto>> CreateAsync(CreateClientDto dto, CancellationToken cancellationToken = default);
    Task<ErrorOr<UserResponseDto>> UpdateAsync(Guid userId, UpdateAdminUserDto dto, CancellationToken cancellationToken = default);
    Task<ErrorOr<UserResponseDto>> PatchAsync(Guid userId, PatchAdminUserDto dto, CancellationToken cancellationToken = default);
    Task<ErrorOr<Success>> DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
}
