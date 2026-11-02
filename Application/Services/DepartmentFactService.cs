using Application.DTO;
using Application.Interfaces;
using Domain.IRepository;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class DepartmentFactService : IDepartmentFactService
{
    private readonly IDepartmentsRepository _departmentsRepository;
    private readonly IFunFactService _funFactService;
    private readonly ILogger<DepartmentFactService> _logger;

    public DepartmentFactService(
        IDepartmentsRepository departmentsRepository,
        IFunFactService funFactService,
        ILogger<DepartmentFactService> logger)
    {
        _departmentsRepository = departmentsRepository;
        _funFactService = funFactService;
        _logger = logger;
    }

    public async Task<List<DepartmentFactResultDto>> ProcessFactsForAllDepartmentsAsync(CancellationToken cancellationToken = default)
    {
        var departments = (await _departmentsRepository.GetAllAsync(cancellationToken)).ToList();
        var results = new List<DepartmentFactResultDto>();
        if (departments.Count == 0)
        {
            _logger.LogInformation("Nuk u gjet asnjë departament për të përditësuar.");
            return results;
        }
        foreach (var dept in departments)
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            try
            {
                // Thërrasim API-në e jashtme për të marrë një fakt interesant
                var fact = await _funFactService.GetRandomFactAsync(cancellationToken);
                string finalFact = !string.IsNullOrWhiteSpace(fact)
                    ? fact
                    : "Fakt interesant nuk është i disponueshëm për momentin.";

                // 1. Përditësojmë vlerën në fushën e entitetit
                dept.DailyFunFact = finalFact;
                // 2. Ruajmë ndryshimet në databazë përmes repository (pa DbContext direkt në shërbim)
                await _departmentsRepository.UpdateAsync(dept, cancellationToken);
                results.Add(new DepartmentFactResultDto
                {
                    DepartmentId = dept.Id,
                    DepartmentName = dept.Name,
                    Fact = finalFact
                });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ndodhi një gabim gjatë ruajtjes së faktit për departamentin {DepartmentName} (ID: {DepartmentId}).", dept.Name, dept.Id);
            }
        }
        return results;
    }

}