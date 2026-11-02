namespace Application.Interfaces;
/// <summary>
/// Abstrakson operacionet e cache (Redis), me fallback automatik në rast dështimi.
/// </summary>

public interface ICacheService
{
    /// <summary>Merr vlerën e cache-it për çelësin dhënë. Kthen null nëse nuk gjendet ose Redis dështon.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    /// <summary>Vendos vlerën në cache me skadim opsional (default: 10 min).</summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    /// <summary>Fshin çelësin e dhënë nga cache. Nuk hedh exception nëse nuk ekziston.</summary>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    /// <summary>Fshin një listë çelësash në mënyrë efikase (batch remove).</summary>
    Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);
}