using Application.Models;
using Application.DTO.Dashboard;
using Application.Interfaces;
using Application.Mappers;
using Domain.IRepository;
using ErrorOr;
using Microsoft.Extensions.Logging;
namespace Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ICacheService _cacheService;
    private readonly IAccessService _accessService;
    private readonly ILogger<DashboardService> _logger;
    public DashboardService(
        IDashboardRepository dashboardRepository,
        ICacheService cacheService,
        IAccessService accessService,
        ILogger<DashboardService> logger)
    {
        _dashboardRepository = dashboardRepository;
        _cacheService = cacheService;
        _accessService = accessService;
        _logger = logger;
    }
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);   
    public async Task<ErrorOr<FullDashboardDto>> GetAdminDashboardAsync(Guid? departmentId = null, CancellationToken cancellationToken = default)
    {
        string cacheKey = departmentId.HasValue
            ? CacheKeys.DashboardDepartment(departmentId.Value)
            : CacheKeys.DashboardGlobalAdmin;
        return await GetDashboardInternalAsync(cacheKey, departmentId, cancellationToken);
    }

    public async Task<ErrorOr<FullDashboardDto>> GetManagerDashboardAsync(CancellationToken cancellationToken = default)
    {
        var deptResult = await _accessService.GetValidatedDepartmentIdAsync(cancellationToken);
        if (deptResult.IsError)
            return deptResult.Errors;
        if (!deptResult.Value.HasValue)
        {
            return Error.Validation("Manager.DepartmentRequired", "Nuk jeni i caktuar në asnjë departament.");
        }
        Guid departmentId = deptResult.Value.Value;
        string cacheKey = CacheKeys.DashboardDepartment(departmentId);
        return await GetDashboardInternalAsync(cacheKey, departmentId, cancellationToken);
    }
    private async Task<FullDashboardDto> GetDashboardInternalAsync(
        string cacheKey,
        Guid? departmentId,
        CancellationToken cancellationToken)
    {
        var cached = await _cacheService.GetAsync<FullDashboardDto>(
            cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Cache HIT — {Key}", cacheKey);
            return cached;
        }
        var totalVal = await _dashboardRepository
            .GetTotalPurchaseValueAsync(departmentId, cancellationToken);
        var deptModels = await _dashboardRepository
            .GetValueByDepartmentAsync(departmentId, cancellationToken);
        var statusModels = await _dashboardRepository
            .GetStatusBreakdownAsync(departmentId, cancellationToken);
        var categoryModels = await _dashboardRepository
            .GetAssetsByCategoryAsync(departmentId, cancellationToken);
        var ageModels = await _dashboardRepository
            .GetAgeDistributionAsync(departmentId, cancellationToken);
        var fullDto = new FullDashboardDto
        {
            TotalPurchaseValue = totalVal,
            ValueByDepartment = deptModels.Select(m => m.ToDto()).ToList(),
            StatusBreakdown = statusModels.Select(m => m.ToDto()).ToList(),
            AssetsByCategory = categoryModels.Select(m => m.ToDto()).ToList(),
            AgeDistribution = ageModels.Select(m => m.ToDto()).ToList()
        };
        await _cacheService.SetAsync(
            cacheKey, fullDto, CacheDuration, cancellationToken);

        return fullDto;
    }
}