using ErrorOr;
namespace Application.Interfaces;

public interface ICurrentUserService
{
    ErrorOr<Guid> GetUserId();
    string? GetRole();
    bool IsAuthenticated();
    bool IsAdmin();
    bool IsManager();
}
