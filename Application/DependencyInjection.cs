using Application.Interfaces;
using Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Application.Background;

namespace Application;
public static class DependencyInjection
{

    public static IServiceCollection AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
    {
        AddValidators(services);
        AddBackgroundServices(services);
        AddAuth(services);
        AddUserServices(services);
        AddStaffServices(services);
        AddRedisCacheService(services, configuration);
        return services;
    }

    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddTransient(typeof(ILoggerService<>), typeof(LoggerService<>));
        return services;
    }

    private static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserAccountService, UserAccountService>();
        return services;
    }

    private static IServiceCollection AddUserServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddTransient<IEmailService, EmailService>();
        services.AddScoped<IAccessService, AccessService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
        return services;
    }

    private static IServiceCollection AddStaffServices(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IAssetsService, AssetsService>();
        services.AddScoped<IDepartmentsService, DepartmentService>();
        return services;
    }

    private static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddHostedService<AssetExpirationBackgroundService>();
        services.AddSingleton<IDashboardWarmupQueue, DashboardWarmupQueue>();
        services.AddHostedService<DashboardWarmupBackgroundService>();
        services.AddHttpClient<IFunFactService, FunFactService>();
        services.AddScoped<IDepartmentFactService, DepartmentFactService>();
        services.AddHostedService<DepartmentFunFactBackgroundService>();
        return services;
    }

    public static IServiceCollection AddRedisCacheService(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = Environment.GetEnvironmentVariable("REDIS")
            ?? configuration.GetConnectionString("Redis");
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "Managment-App_";
        });
        services.AddScoped<ICacheService, RedisCacheService>();
        return services;
    }

}