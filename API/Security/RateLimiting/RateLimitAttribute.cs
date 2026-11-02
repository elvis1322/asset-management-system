using System.Security.Claims;
using System.Threading.RateLimiting;


namespace API.Security.RateLimiting;

public static class RateLimitConfiguration
{
    public const string LoginPolicy = "LoginPolicy";
    public const string RegisterPolicy = "RegisterPolicy";
    public const string ReadPolicy = "ReadPolicy";
    public const string WritePolicy = "WritePolicy";
    public const string SensitivePolicy = "SensitivePolicy";

    public static IServiceCollection RegisterRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;

            // 1. Global limit: krejt API, sipas IP-se.
            options.GlobalLimiter =
                PartitionedRateLimiter.Create<HttpContext, string>(
                    context =>
                    {
                        var ip = GetClientIp(context);

                        return RateLimitPartition.GetFixedWindowLimiter(
                            ip,
                            _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 600,
                                Window = TimeSpan.FromMinutes(1),
                                QueueLimit = 0,
                                AutoReplenishment = true
                            });
                    });

            // 2. Limite specifike sipas endpoint-it dhe identitetit.
            options.AddPolicy(
                LoginPolicy,
                CreatePolicy(6, TimeSpan.FromMinutes(1)));

            options.AddPolicy(
                RegisterPolicy,
                CreatePolicy(4, TimeSpan.FromMinutes(5)));

            options.AddPolicy(
                ReadPolicy,
                CreatePolicy(120, TimeSpan.FromMinutes(1)));

            options.AddPolicy(
                WritePolicy,
                CreatePolicy(30, TimeSpan.FromMinutes(1)));

            options.AddPolicy(
                SensitivePolicy,
                CreatePolicy(5, TimeSpan.FromMinutes(1)));
        });

        return services;
    }

    private static Func<HttpContext, RateLimitPartition<string>>
        CreatePolicy(int permitLimit, TimeSpan window)
    {
        return context =>
        {
            var ip = GetClientIp(context);
            // UserId kur perdoruesi eshte i autentikuar.
            // IP kur kerkesa eshte anonime.
            var userId =
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? context.User.FindFirst("sub")?.Value;

            var identityKey = !string.IsNullOrWhiteSpace(userId)
                ? $"user:{userId}"
                : $"ip:{ip}";

            // Perdore route template, jo GUID-ne konkrete.
            var endpointKey =
                (context.GetEndpoint() as RouteEndpoint)?
                    .RoutePattern.RawText
                ?? context.Request.Path.Value
                ?? "/";

            var partitionKey = $"{identityKey}:{endpointKey}";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey,
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = window,
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
        };
    }

    private static string GetClientIp(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";
    }
}
