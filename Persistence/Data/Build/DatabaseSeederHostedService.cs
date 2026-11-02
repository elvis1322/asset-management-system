using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Persistence.Data.Build.Seed;
namespace Persistence.Services;

public class DatabaseSeederHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    public DatabaseSeederHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<DatabaseSeederHostedService>>();
        try
        {
            logger.LogInformation("Duke ekzekutuar Database Migrations...");
            var context = services.GetRequiredService<DataContext>();
            var roleManager = services.GetRequiredService<RoleManager<Role>>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            await context.Database.MigrateAsync(cancellationToken);
            await RoleSeed.SeedAsync(roleManager);
            await UserRoleSeeder.SeedAsync(userManager);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GABIM KRITIK GJATË SEEDING: {Message}", ex.Message);
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}