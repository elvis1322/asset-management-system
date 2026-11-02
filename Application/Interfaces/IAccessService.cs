using ErrorOr;
namespace Application.Interfaces;

public interface IAccessService
{
    ErrorOr<Guid> GetCurrentUserId();
    ErrorOr<string?> GetCurrentUserRole();
    Task<ErrorOr<Guid>> GetCurrentEmployeeIdAsync(CancellationToken cancellationToken = default);
    Task<ErrorOr<Guid?>> GetValidatedDepartmentIdAsync(CancellationToken cancellationToken = default);
    bool IsAuthenticated();
    bool IsAdmin();
    bool IsManager();
}
