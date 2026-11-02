using System.Security.Claims;
using API.Security.RateLimiting;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _dashboardService;
    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }
    /// <summary>
    /// Kthen të dhënat e plota të dashboard-it për Administratorin, me opsion filtri sipas departamentit.
    /// </summary>
    [HttpGet("Admin/GetFullDashboard")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.ReadPolicy)]
    public async Task<IActionResult> GetAdminDashboard([FromQuery] Guid? departmentId = null, CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetAdminDashboardAsync(departmentId, cancellationToken);
        return HandleResult(result, "Dashboard u morr me sukses.");
    }
    /// <summary>
    /// Kthen të dhënat e plota të dashboard-it për Menaxherin e autorizuar.
    /// </summary>
    [HttpGet("Manager/GetFullDashboard")]
    [Authorize(Roles = "Manager")]
    [EnableRateLimiting(RateLimitConfiguration.ReadPolicy)]
    public async Task<IActionResult> GetManagerDashboard(CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetManagerDashboardAsync(cancellationToken);
        return HandleResult(result, "Dashboard u morr me sukses.");
    }
}