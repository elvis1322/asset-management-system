using ErrorOr;
using Application.DTO.Dashboard;
namespace Application.Interfaces;

public interface IDashboardService
{
    Task<ErrorOr<FullDashboardDto>> GetAdminDashboardAsync(Guid? departmentId = null, CancellationToken cancellationToken = default);
    Task<ErrorOr<FullDashboardDto>> GetManagerDashboardAsync( CancellationToken cancellationToken = default);
}