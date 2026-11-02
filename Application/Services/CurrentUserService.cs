using System.Security.Claims;
using Application.Interfaces;
using ErrorOr;
using Microsoft.AspNetCore.Http;
namespace Application.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public ErrorOr<Guid> GetUserId()
    {
        var userId =
            User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User?.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Error.Unauthorized(
                "User.NotAuthenticated",
                "Përdoruesi nuk është i autentikuar.");
        }
        if (!Guid.TryParse(userId, out var parsedUserId))
        {
            return Error.Validation(
                "User.InvalidId",
                "ID-ja e përdoruesit nuk është valide.");
        }
        return parsedUserId;
    }
    public string? GetRole()
    {
        return User?.FindFirst(ClaimTypes.Role)?.Value
            ?? User?.FindFirst("role")?.Value;
    }

    public bool IsAuthenticated()
    {
        return User?.Identity?.IsAuthenticated == true;
    }

    public bool IsAdmin()
    {
        return GetRole() == "Admin";
    }

    public bool IsManager()
    {
        return GetRole() == "Manager";
    }
}