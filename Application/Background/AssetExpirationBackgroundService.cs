using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Domain.IRepository;

namespace Application.Background;

public class AssetExpirationBackgroundService : BackgroundService
{
    private readonly ILogger<AssetExpirationBackgroundService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24);

    public AssetExpirationBackgroundService(
        ILogger<AssetExpirationBackgroundService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Asset Expiration Background Service u nis me sukses.");
        // I japim kohë aplikacionit të inicializohet
        await Task.Yield();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAssetExpirationsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ndodhi një gabim gjatë kontrollit të skadimit të aseteve në prapaskenë.");
            }
            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckAssetExpirationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var assetRepository = scope.ServiceProvider.GetRequiredService<IAssetsRepository>();

        int totalExpiringFound = 0;

        // 1. Kontrollo për pragjet në të ardhmen (30, 20, 10, 5, etj.)
        int[] thresholds = { 30, 20, 10, 5, 4, 3, 2, 1, 0 };

        foreach (var days in thresholds)
        {
            var expiringAssets = await assetRepository.GetAssetsExpiringInDaysAsync(days, cancellationToken);
            foreach (var asset in expiringAssets)
            {
                totalExpiringFound++;
                _logger.LogWarning("ALARM: Aseti '{AssetName}' (Tag: {AssetTag}) skadon pas {Days} ditësh!", asset.Name, asset.AssetTag, days);
            }
        }

        // 2. Kontrollo për asetet që KANË SKADUAR tashmë
        var expiredAssets = await assetRepository.GetExpiredAssetsAsync(cancellationToken);
        foreach (var asset in expiredAssets)
        {
            totalExpiringFound++;
            _logger.LogWarning("ALARM: Aseti '{AssetName}' (Tag: {AssetTag}) i ka skaduar garancia!", asset.Name, asset.AssetTag);
        }

        if (totalExpiringFound == 0)
        {
            _logger.LogInformation("Kontrolli i aseteve përfundoi: Asnjë aset nuk është në prag skadimi ose i skaduar.");
        }
        else
        {
            _logger.LogInformation("Kontrolli i aseteve përfundoi: U gjetën gjithsej {Count} asete (që po skadojnë ose kanë skaduar).", totalExpiringFound);
        }
    }
}
