namespace Application.Interfaces;

public interface ICacheInvalidationService
{
  Task InvalidateDashboardCacheAsync(IEnumerable<Guid?> departmentIds, CancellationToken cancellationToken = default);
}