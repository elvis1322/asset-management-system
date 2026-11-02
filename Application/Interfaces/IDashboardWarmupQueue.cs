namespace Application.Interfaces;

public interface IDashboardWarmupQueue
{
   ValueTask EnqueueWarmupAsync(Guid? departmentId, string role, CancellationToken cancellationToken = default);  
    ValueTask<(Guid? DepartmentId, string Role)> DequeueAsync(CancellationToken cancellationToken = default);
}