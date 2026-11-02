using Application.Models;
using Application.Interfaces;

namespace Application.Services;

public class CacheInvalidationService(
    ICacheService cacheService,
    IDashboardWarmupQueue warmupQueue)
    : ICacheInvalidationService
{
    public async Task InvalidateDashboardCacheAsync(
        IEnumerable<Guid?> departmentIds,
        CancellationToken cancellationToken = default)
    {
        var departments = departmentIds
            .Where(id => id.HasValue && id.Value != Guid.Empty)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var keysToRemove = new List<string>
        {
            CacheKeys.DashboardGlobalAdmin
        };
        keysToRemove.AddRange(departments.Select(CacheKeys.DashboardDepartment));
        await cacheService.RemoveManyAsync(keysToRemove, cancellationToken);
        foreach (var departmentId in departments)
        {
            await warmupQueue.EnqueueWarmupAsync(departmentId, "Manager", cancellationToken);
        }
        await warmupQueue.EnqueueWarmupAsync(null, "Admin", cancellationToken);
    }
}