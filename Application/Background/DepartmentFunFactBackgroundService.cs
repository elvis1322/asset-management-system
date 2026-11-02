using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class DepartmentFunFactBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DepartmentFunFactBackgroundService> _logger;

    public DepartmentFunFactBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<DepartmentFunFactBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Department FunFact Background Service u nis me sukses.");
        // Lëshon thread-in e startup-it që Web Host të përfundojë inicializimin pa u bllokuar
        await Task.Yield();
        // 1. Ekzekutimi i parë kryhet MENJËHERË sapo të ngrihet aplikacioni (pa pritur 24 orë)
        await RunProcessFactsAsync(stoppingToken);
        // 2. Vazhdon të ekzekutohet automatikisht çdo 24 orë me PeriodicTimer
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        try
        {
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunProcessFactsAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Department FunFact Background Service po ndërpritet për shkak të ndalimit të aplikacionit.");
        }
    }

    private async Task RunProcessFactsAsync(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
            return;
        try
        {
            _logger.LogInformation("Duke nisur ciklin e përditësimit të fun facts për të gjitha departamentet...");

            // Krijojmë një Scope të ri pasi BackgroundService është Singleton kurse IDepartmentFactService është Scoped
            using var scope = _scopeFactory.CreateScope();
            var departmentFactService = scope.ServiceProvider.GetRequiredService<IDepartmentFactService>();

            await departmentFactService.ProcessFactsForAllDepartmentsAsync(stoppingToken);

            _logger.LogInformation("Cikli i përditësimit të fun facts për të gjitha departamentet përfundoi me sukses.");
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Aplikacioni po mbyllet, anashkalohet si gabim
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ndodhi një gabim gjatë ekzekutimit të Department FunFact Background Service.");
        }
    }
}