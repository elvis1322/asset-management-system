using Application.Interfaces;
using Microsoft.Extensions.Logging;
namespace Application.Services;

public class LoggerService<T>(ILogger<T> logger) : ILoggerService<T>
{
    private readonly ILogger<T> _logger = logger;

    public void LogInformation(string message, params object?[] args)
        => _logger.LogInformation(message, args);

    public void LogWarning(string message, params object?[] args)
        => _logger.LogWarning(message, args);

    public void LogError(Exception exception, string message, params object?[] args)
        => _logger.LogError(exception, message, args);

    public void LogError(string message, params object?[] args)
        => _logger.LogError(message, args);

    public void LogDebug(string message, params object?[] args)
        => _logger.LogDebug(message, args);
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => _logger.BeginScope(state);
}