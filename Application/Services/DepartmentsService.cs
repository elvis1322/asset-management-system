using Application.Models;
using Application.DTO.Departments;
using Application.Interfaces;
using Domain.Entities;
using Domain.IRepository;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Application.Mappers;
namespace Application.Services;

public class DepartmentService : IDepartmentsService
{
    private readonly IDepartmentsRepository _departmentRepository;
    private readonly ILogger<DepartmentService> _logger;
    private readonly IAccessService _accessService;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    public DepartmentService(
        IDepartmentsRepository departmentRepository,
        ILogger<DepartmentService> logger,
        IAccessService accessService,
        ICacheInvalidationService cacheInvalidationService)
    {
        _departmentRepository = departmentRepository;
        _logger = logger;
        _accessService = accessService;
        _cacheInvalidationService = cacheInvalidationService;
    }

    public async Task<Department?> GetByIdTrackingAsync(Guid id, CancellationToken cancellationToken = default)
        => await _departmentRepository.GetByIdTrackingAsync(id, cancellationToken);

    public async Task<bool> IsEmployeeInDepartmentAsync(Guid departmentId, Guid employeeId, CancellationToken cancellationToken)
            => await _departmentRepository.IsEmployeeInDepartmentAsync(departmentId, employeeId, cancellationToken);

    public async Task<bool> IsAssetInDepartmentAsync(Guid departmentId, Guid assetId, CancellationToken cancellationToken)
        => await _departmentRepository.IsAssetInDepartmentAsync(departmentId, assetId, cancellationToken);

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty) return false;
        var dept = await _departmentRepository.GetByIdNoTrackingAsync(id, cancellationToken);
        return dept != null;
    }

    public async Task<ErrorOr<PagedResult<DepartmentResponseDto>>> GetPagedAsync(
    PaginationParams paginationParams,
    string? term,
    CancellationToken cancellationToken = default)
    {
        string? sanitizedTerm = null;
        if (!string.IsNullOrWhiteSpace(term))
        {
            sanitizedTerm = term.Trim();
            if (sanitizedTerm.Length > 100)
                sanitizedTerm = sanitizedTerm[..100];
        }
        var (departments, totalCount) = await _departmentRepository.GetPagedDepartmentsAsync(
            sanitizedTerm,
            paginationParams.PageNumber,
            paginationParams.PageSize,
            cancellationToken);
        var departmentList = departments.ToList();
        var itemDtos = departments.Select(d => d.ToDto()).ToList();
        return PagedResultExtensions.CreatePagedResult<Department, DepartmentResponseDto>(itemDtos, totalCount, paginationParams);
    }

    public async Task<ErrorOr<DepartmentResponseDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdNoTrackingAsync(id, cancellationToken);
        if (department is null)
        {
            _logger.LogWarning("Departamenti me ID {DeptId} nuk u gjet.", id);
            return Error.NotFound("Department.NotFound", "Departamenti nuk u gjet.");
        }
        return department.ToDto();
    }

    public async Task<ErrorOr<DepartmentResponseDto>> CreateAsync(
      CreateDepartmentDto dto,
      CancellationToken cancellationToken = default)
    {
        var deptResult = await _accessService.GetValidatedDepartmentIdAsync(cancellationToken);
        if (deptResult.IsError)
        {
            return deptResult.Errors;
        }
        if (deptResult.Value.HasValue)
        {
            _logger.LogWarning("Manageri tentoi të krijojë një departament të ri.");
            return Error.Validation("Manager.Unauthorized", "Menaxherët nuk kanë leje të krijojnë departamente të reja.");
        }
        var existing = await _departmentRepository.GetByNameAsync(dto.Name, cancellationToken);
        if (existing != null)
        {
            _logger.LogWarning("Konflikt: departamenti '{Name}' ekziston tashmë.", dto.Name);
            return Error.Conflict("Department.Exists", "Një departament me këtë emër ekziston tashmë.");
        }
        var department = new Department
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Code = dto.Code,
            Description = dto.Description
        };
        await _departmentRepository.AddAsync(department, cancellationToken);
        await _cacheInvalidationService.InvalidateDashboardCacheAsync([ department.Id ], cancellationToken);
        _logger.LogInformation("U krijua departamenti {DeptId} ('{Name}').", department.Id, department.Name);
        return department.ToDto();
    }

    public async Task<ErrorOr<DepartmentResponseDto>> UpdateAsync(
        Guid id,
        UpdateDepartmentDto dto,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Error.Validation("Department.Required", "Administratori duhet të specifikojë ID-në e departamentit.");
        }
        var department = await _departmentRepository.GetByIdTrackingAsync(id, cancellationToken);
        if (department is null)
        {
            _logger.LogWarning("Tentativë update — departamenti {DeptId} nuk u gjet.", id);
            return Error.NotFound("Department.NotFound", "Departamenti nuk u gjet.");
        }
        var existingWithSameName = await _departmentRepository.GetByNameAsync(dto.Name, cancellationToken);
        if (existingWithSameName != null && existingWithSameName.Id != id)
        {
            _logger.LogWarning("Konflikt: departament tjetër me emrin '{Name}' ekziston.", dto.Name);
            return Error.Conflict("Department.Exists", "Një departament tjetër me këtë emër ekziston tashmë.");
        }
        department.Name = dto.Name;
        department.Description = dto.Description;
        await _departmentRepository.UpdateAsync(department, cancellationToken);
        await _cacheInvalidationService.InvalidateDashboardCacheAsync([id], cancellationToken);
        _logger.LogInformation("U përditësua departamenti {DeptId}.", id);
        return department.ToDto();
    }

    public async Task<ErrorOr<DepartmentResponseDto>> UpdateCodeAsync(
        Guid id,
        UpdateDepartamentCodeDto dto,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
        {
            return Error.Validation("Department.Required", "Administratori duhet të specifikojë ID-në e departamentit.");
        }
        var department = await _departmentRepository.GetByIdTrackingAsync(id, cancellationToken);
        if (department is null)
        {
            _logger.LogWarning("Tentativë update kodi — departamenti {DeptId} nuk u gjet.", id);
            return Error.NotFound("Department.NotFound", "Departamenti nuk u gjet.");
        }
        var existingWithSameCode = await _departmentRepository.GetByCodeAsync(dto.Code, cancellationToken);
        if (existingWithSameCode != null && existingWithSameCode.Id != id)
        {
            _logger.LogWarning("Konflikt: departament tjetër me kodin '{Code}' ekziston.", dto.Code);
            return Error.Conflict("Department.CodeExists", "Një departament tjetër me këtë kod ekziston tashmë.");
        }
        department.Code = dto.Code.Trim();
        await _departmentRepository.UpdateAsync(department, cancellationToken);
        await _cacheInvalidationService.InvalidateDashboardCacheAsync([id], cancellationToken);
        _logger.LogInformation("U përditësua kodi i departamentit {DeptId}.", id);
        return department.ToDto();
    }

    public async Task<ErrorOr<DepartmentResponseDto>> UpdateByManagerAsync(
        UpdateDepartmentDto dto,
        CancellationToken cancellationToken = default)
    {
        var deptResult = await _accessService.GetValidatedDepartmentIdAsync(cancellationToken);
        if (deptResult.IsError)
            return deptResult.Errors;
        if (!deptResult.Value.HasValue)
        {
            return Error.Validation("Manager.DepartmentRequired", "Nuk jeni i caktuar në asnjë departament.");
        }
        Guid departmentId = deptResult.Value.Value;
        var department = await _departmentRepository.GetByIdTrackingAsync(departmentId, cancellationToken);
        if (department is null)
        {
            _logger.LogWarning("Departamenti i menaxherit me ID {DeptId} nuk u gjet.", departmentId);
            return Error.NotFound("Department.NotFound", "Departamenti nuk u gjet.");
        }
        var existingWithSameName = await _departmentRepository.GetByNameAsync(dto.Name, cancellationToken);
        if (existingWithSameName != null && existingWithSameName.Id != departmentId)
        {
            _logger.LogWarning("Konflikt: departament tjetër me emrin '{Name}' ekziston.", dto.Name);
            return Error.Conflict("Department.Exists", "Një departament tjetër me këtë emër ekziston tashmë.");
        }
        department.Name = dto.Name;
        department.Description = dto.Description;
        await _departmentRepository.UpdateAsync(department, cancellationToken);
        await _cacheInvalidationService.InvalidateDashboardCacheAsync([departmentId], cancellationToken);
        _logger.LogInformation("Menaxheri përditësoi departamentin e tij {DeptId}.", departmentId);
        return department.ToDto();
    }

    public async Task<ErrorOr<Success>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var roleResult = _accessService.GetCurrentUserRole();
        if (roleResult.IsError)
            return roleResult.Errors;
        if (roleResult.Value == "Manager")
        {
            _logger.LogWarning("Manageri tentoi të fshijë departamentin {DeptId}.", id);
            return Error.Validation("Department.Unauthorized", "Menaxherët nuk kanë leje të fshijnë departamente.");
        }
        var department = await _departmentRepository.GetByIdTrackingAsync(id, cancellationToken);
        if (department is null)
        {
            _logger.LogWarning("Tentativë fshirje — departamenti {DeptId} nuk u gjet.", id);
            return Error.NotFound("Department.NotFound", "Departamenti nuk u gjet.");
        }
        if ((department.Employees?.Count > 0) || (department.Assets?.Count > 0))
        {
            _logger.LogWarning("Departamenti {DeptId} ka punonjës ose asete — nuk fshihet.", id);
            return Error.Conflict("Department.NotEmpty",
                "Nuk mund të fshihet departamenti pasi ka punonjës ose asete të lidhura me të.");
        }
        await _departmentRepository.DeleteAsync(department, cancellationToken);
        await _cacheInvalidationService.InvalidateDashboardCacheAsync([id], cancellationToken);
        _logger.LogInformation("U fshi departamenti {DeptId}.", id);
        return Result.Success;
    }

    public async Task UpdateDepartmentFunFactAsync(Guid departmentId, string funFact, CancellationToken cancellationToken = default)
    {
        if (departmentId == Guid.Empty) return;
        var department = await _departmentRepository.GetByIdTrackingAsync(departmentId, cancellationToken);
        if (department == null)
        {
            _logger.LogWarning("Departamenti me ID {DeptId} nuk u gjet për përditësimin e fun fact.", departmentId);
            return;
        }
        department.DailyFunFact = funFact;
        await _departmentRepository.UpdateAsync(department, cancellationToken);
    }
}