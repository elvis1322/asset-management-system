using System.Text.Json;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
namespace Application.Services;

public class RedisCacheService(
    IDistributedCache cache,
    ILogger<RedisCacheService> logger) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedData = await cache.GetStringAsync(key, cancellationToken);
            if (string.IsNullOrEmpty(cachedData))
                return default;
            logger.LogDebug(" Cache HIT për çelësin '{Key}'.", key);
            return JsonSerializer.Deserialize<T>(cachedData, JsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex,
                " Dështoi deserializimi i cache-it për çelësin '{Key}'. Çelësi po fshihet për rifreskim...", key);
            await RemoveAsync(key, cancellationToken);
            return default;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                 " Gabim gjatë leximit nga Redis për çelësin '{Key}'. Fallback te SQL Server...", key);
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
            };
            var serializedData = JsonSerializer.Serialize(value, JsonOptions);
            await cache.SetStringAsync(key, serializedData, options, cancellationToken);
            logger.LogDebug("💾 Cache SET për çelësin '{Key}' (TTL: {TTL}).", key, expiration ?? TimeSpan.FromMinutes(5));
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                " Dështoi ruajtja e të dhënave në Redis për çelësin '{Key}'. Të dhënat do shërbehen nga DB.", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await cache.RemoveAsync(key, cancellationToken);
            logger.LogDebug(" Cache REMOVE për çelësin '{Key}'.", key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                " Dështoi fshirja e çelësit '{Key}' nga Redis. Cache mund të jetë i vjetëruar.", key);
        }
    }

    public async Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        var tasks = keys.Select(key => RemoveAsync(key, cancellationToken));
        await Task.WhenAll(tasks);
    }
}