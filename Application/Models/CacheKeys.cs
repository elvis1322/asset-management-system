namespace Application.Models;
public static class CacheKeys
{
    public const string DashboardGlobalAdmin = "dashboard:global:admin";
    public static string DashboardDepartment(Guid id) => $"dashboard:dept:{id}";
    public static string RevokedRefreshToken(string tokenValue) => $"auth:revoked:{tokenValue}";
}