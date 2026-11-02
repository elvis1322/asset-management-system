using System.Collections.Concurrent;
using System.Threading.Channels;
using Application.Interfaces;
namespace Application.Services;

public class DashboardWarmupQueue : IDashboardWarmupQueue 
{
    private readonly Channel<(Guid? DepartmentId, string Role)> _channel = 
        Channel.CreateUnbounded<(Guid? DepartmentId, string Role)>(new UnboundedChannelOptions 
        { 
            SingleReader = true 
        });  
    private readonly ConcurrentDictionary<string, bool> _pendingTasks = new();

    public ValueTask EnqueueWarmupAsync(Guid? departmentId, string role, CancellationToken cancellationToken = default) 
    {
        var taskKey = $"{role}_{departmentId?.ToString() ?? "Global"}";
        if (_pendingTasks.TryAdd(taskKey, true))
        {
            _channel.Writer.TryWrite((departmentId, role));
        }
        return ValueTask.CompletedTask; 
    } 

    public async ValueTask<(Guid? DepartmentId, string Role)> DequeueAsync(CancellationToken cancellationToken = default) 
    {
        var item = await _channel.Reader.ReadAsync(cancellationToken);
        var taskKey = $"{item.Role}_{item.DepartmentId?.ToString() ?? "Global"}";
        _pendingTasks.TryRemove(taskKey, out _);
        return item;
    } 
}