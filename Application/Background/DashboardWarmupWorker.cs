using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace Application.Services;

public class DashboardWarmupBackgroundService( 
    IDashboardWarmupQueue queue, 
    IServiceScopeFactory scopeFactory, 
    ILogger<DashboardWarmupBackgroundService> logger) : BackgroundService 
{ 
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
    { 
        logger.LogInformation("Dashboard Warmup Background Service started."); 
        while (!stoppingToken.IsCancellationRequested) 
        { 
            try 
            { 
                var (departmentId, role) = await queue.DequeueAsync(stoppingToken); 
                using var scope = scopeFactory.CreateScope(); 
                var dashboardService = scope.ServiceProvider.GetRequiredService<IDashboardService>(); 
                logger.LogInformation("Warming up dashboard for Role: {Role}, Dept: {DeptId}", role, departmentId ?? Guid.Empty); 
                await dashboardService.GetAdminDashboardAsync(departmentId, stoppingToken); 
                logger.LogInformation("Dashboard warmup completed successfully for Role: {Role}", role); 
            } 
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) 
            { 
                break; 
            } 
            catch (Exception ex) 
            { 
                logger.LogError(ex, "Error occurred during dashboard warmup execution."); 
            } 
        } 
    } 
}