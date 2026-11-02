using Application.DTO;
namespace Application.Interfaces;

public interface IDepartmentFactService
{
    Task<List<DepartmentFactResultDto>> ProcessFactsForAllDepartmentsAsync(CancellationToken cancellationToken = default);
}