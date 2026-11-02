using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Data;
using Persistence.Services;
using Persistence.Repositories;
using Domain.IRepository;
using AssetManagementSystem.Infrastructure.Interceptors;

namespace Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistenceLayer(this IServiceCollection services, IConfiguration configuration)
    {
        AddInterceptor(services);
        AddDatabaseSeederHostedService(services, configuration);
        AddIdentityServices(services);
        AddUserRepository(services);
        AddStaffRepository(services);
        return services;
    }
    
    private static IServiceCollection AddDatabaseSeederHostedService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<DatabaseSeederHostedService>();
        services.AddDbContext<DataContext>((sp, options) =>
    {
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
               .AddInterceptors(sp.GetRequiredService<AuditExecuteUpdateInterceptor>());
    });
        return services;
    }

    private static IServiceCollection AddInterceptor(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<AuditExecuteUpdateInterceptor>();
        return services;
    }

    private static IServiceCollection AddUserRepository(this IServiceCollection services)
    {
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        return services;
    }

    private static IServiceCollection AddStaffRepository(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IAssetsRepository, AssetsRepository>();
        services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();
        return services;
    }

    private static void AddIdentityServices(IServiceCollection services)
    {
        services.AddIdentityCore<User>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(7);
            options.Lockout.MaxFailedAccessAttempts = 5;
        })
        .AddRoles<Role>()
        .AddRoleManager<RoleManager<Role>>()
        .AddEntityFrameworkStores<DataContext>()
        .AddDefaultTokenProviders();
    }
}