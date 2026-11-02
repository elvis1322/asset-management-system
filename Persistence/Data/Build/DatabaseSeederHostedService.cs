using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Persistence.Data;
using Persistence.Data.Build.Seed;
using Persistence.Seeders; // <--- Shto këtë emërhapësirë (namespace) për SP & Triggers

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
            logger.LogInformation("Duke ekzekutuar Database Migrations dhe Seeding...");
            var context = services.GetRequiredService<DataContext>();
            var roleManager = services.GetRequiredService<RoleManager<Role>>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            
            // 1. Migrimet
            await context.Database.MigrateAsync(cancellationToken);
            
            // 2. Seed i Roleve dhe Përdoruesve
            await RoleSeed.SeedAsync(roleManager);
            await UserRoleSeeder.SeedAsync(userManager);

            // 3. Seed i Stored Procedures dhe Triggers
            await StoredProcedureSeeder.SeedAsync(context);
            await TriggerSeeder.SeedAsync(context);

            logger.LogInformation("Database seeding dhe konfigurimi i objektit u krye me sukses!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "GABIM KRITIK GJATË SEEDING: {Message}", ex.Message);
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}