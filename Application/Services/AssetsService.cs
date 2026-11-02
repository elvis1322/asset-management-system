using Application.DTO.Assets;
using Application.Interfaces;
using Domain.Entities;
using Domain.IRepository;
using ErrorOr;
using Microsoft.Extensions.Logging;
using Application.Mappers;
using Application.Models;

namespace Application.Services;
/// <summary>
/// Shërbim për menaxhimin e aseteve, caktimet, transferimet dhe kontrollin e qasjes sipas departamenteve.
/// </summary>
public class AssetsService : IAssetsService
{
    private readonly IAssetsRepository _assetRepository;
    private readonly ILogger<AssetsService> _logger;
    private readonly IDepartmentsService _departmentsService;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccessService _accessService;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly IEmployeeRepository _employeeRepository;

    public AssetsService(
        IAssetsRepository assetRepository,
        ILogger<AssetsService> logger,
        IDepartmentsService departmentsService,
        ITransactionRepository transactionRepository,
        IAccessService accessService,
        ICacheInvalidationService cacheInvalidationService,
        IEmployeeRepository employeeRepository)
    {
        _assetRepository = assetRepository;
        _logger = logger;
        _departmentsService = departmentsService;
        _transactionRepository = transactionRepository;
        _accessService = accessService;
        _cacheInvalidationService = cacheInvalidationService;
        _employeeRepository = employeeRepository;
    }

    /// <summary>
    /// Kthen listën e paginuar të aseteve bazuar në departamentin e përdoruesit dhe filtrat e kërkimit.
    /// </summary>
    public async Task<ErrorOr<PagedResult<AssetResponseDto>>> GetAssetsPagedAsync(
        PaginationParams paginationParams,
        string? term,
        AssetStatus? status,
        AssetCategory? category,
        CancellationToken cancellationToken = default)
    {
        // 1. Validimi i departamentit të përdoruesit aktual
        var deptResult = await _accessService.GetValidatedDepartmentIdAsync(cancellationToken);
        if (deptResult.IsError) return deptResult.Errors;
        Guid? effectiveDepartmentId = deptResult.Value;
        string? sanitizedTerm = null;
        if (!string.IsNullOrWhiteSpace(term))
        {
            sanitizedTerm = term.Trim();
            if (sanitizedTerm.Length > 100) sanitizedTerm = sanitizedTerm[..100];
        }
        // 2. Marrja e të dhënave nga baza përmes repository
        var (assets, totalCount) = await _assetRepository.GetPagedAssetsAsync(
            sanitizedTerm, status, category, effectiveDepartmentId,
            paginationParams.PageNumber, paginationParams.PageSize, cancellationToken);
        var assetList = assets.ToList();
        var itemDtos = assetList.Select(a => a.AssetResponseDto()).ToList();
        return PagedResultExtensions.CreatePagedResult<Asset, AssetResponseDto>(itemDtos, totalCount, paginationParams);
    }

    /// <summary>
    /// Kthen asetet e caktuara për një punonjës specifik, duke kryer verifikimet e autorizimit.
    /// </summary>
    public async Task<ErrorOr<PagedResult<AssetByEmployeeResponseDto>>> GetAssetsByEmployeeAsync(
        Guid employeeId,
        PaginationParams paginationParams,
        CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdNoTrackingAsync(employeeId, cancellationToken);
        if (employee is null)
        {
            return Error.NotFound("Employee.NotFound", "Punonjësi nuk u gjet.");
        }
        var deptResult = await _accessService.GetValidatedDepartmentIdAsync(cancellationToken);
        if (deptResult.IsError) return deptResult.Errors;
        Guid? currentUserDepartmentId = deptResult.Value;
        // Sigurohemi që menaxheri të mos shikojë punonjës jashtë departamentit të tij
        if (currentUserDepartmentId.HasValue && employee.DepartmentId != currentUserDepartmentId.Value)
        {
            return Error.Forbidden("Access.Denied", "Nuk keni leje të shikoni asetet e një punonjësi jashtë departamentit tuaj.");
        }
        var (assets, totalCount) = await _assetRepository.GetPagedByEmployeeIdAsync(
            employeeId, paginationParams.PageNumber, paginationParams.PageSize, cancellationToken);
        var itemDtos = assets.Select(a => a.AssetToEmployeeResponseDto()).ToList();
        return PagedResultExtensions.CreatePagedResult<Asset, AssetByEmployeeResponseDto>(itemDtos, totalCount, paginationParams);
    }

    public async Task<ErrorOr<PagedResult<AssetByDepartmentResponseDto>>> GetAssetsByDepartmentAsync(
        Guid departmentId,
        PaginationParams paginationParams,
        AssetStatus? status,
        AssetCategory? category,
        CancellationToken cancellationToken = default)
    {
        var deptResult = await _accessService.GetValidatedDepartmentIdAsync(cancellationToken);
        if (deptResult.IsError) return deptResult.Errors;
        Guid? currentUserDepartmentId = deptResult.Value;
        if (currentUserDepartmentId.HasValue && currentUserDepartmentId.Value != departmentId)
        {
            _logger.LogWarning("Manageri tentoi të shikojë asetet e departamentit {RequestedDept}", departmentId);
            return Error.Forbidden("Access.Denied", "Nuk keni leje të shikoni asetet e këtij departamenti.");
        }
        var (assets, totalCount) = await _assetRepository.GetPagedByDepartmentIdAsync(
            departmentId, status, category, paginationParams.PageNumber, paginationParams.PageSize, cancellationToken);
        var itemDtos = assets.Select(a => a.AssetToDepartmentResponseDto()).ToList();
        return PagedResultExtensions.CreatePagedResult<Asset, AssetByDepartmentResponseDto>(itemDtos, totalCount, paginationParams);
    }

    public async Task<ErrorOr<AssetResponseDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetByIdNoTrackingAsync(id, cancellationToken);
        if (asset is null)
        {
            return Error.NotFound("Asset.NotFound", "Aseti nuk u gjet.");
        }
        var deptResult = await _accessService.GetValidatedDepartmentIdAsync(cancellationToken);
        if (deptResult.IsError) return deptResult.Errors;
        if (deptResult.Value.HasValue && asset.DepartmentId != deptResult.Value.Value)
        {
            return Error.Validation("Asset.Unauthorized", "Nuk keni leje të shikoni asete jashtë departamentit tuaj.");
        }
        return asset.AssetResponseDto();
    }

    public async Task<ErrorOr<List<AssetResponseDto>>> GetMyAssetsAsync(
        CancellationToken cancellationToken = default)
    {
        var employeeIdResult = await _accessService.GetCurrentEmployeeIdAsync(cancellationToken);
        if (employeeIdResult.IsError) return employeeIdResult.Errors;
        var employeeId = employeeIdResult.Value;
        var assets = await _assetRepository.GetAssetsByEmployeeIdAsync(employeeId, cancellationToken);
        return assets.Select(a => a.AssetResponseDto()).ToList();
    }

    public async Task<ErrorOr<PagedResult<AssetAssignmentHistoryDto>>> GetHistoryAsync(Guid assetId, PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetByIdNoTrackingAsync(assetId, cancellationToken);
        if (asset is null) return Error.NotFound("Asset.NotFound", "Aseti nuk u gjet.");
        var deptResult = await _accessService.GetValidatedDepartmentIdAsync(cancellationToken);
        if (deptResult.IsError) return deptResult.Errors;
        if (deptResult.Value.HasValue && asset.DepartmentId != deptResult.Value.Value)
        {
            return Error.Validation("Asset.Unauthorized", "Nuk keni leje të shikoni historikun jashtë departamentit tuaj.");
        }
        var (items, totalCount) = await _assetRepository.GetAssignmentHistoryPagedAsync(
            assetId, pagination.PageNumber, pagination.PageSize, cancellationToken);
        var dtos = items.Select(h => new AssetAssignmentHistoryDto
        {
            Id = h.Id,
            EmployeeName = h.Employee != null ? $"{h.Employee.FirstName} {h.Employee.LastName}" : string.Empty,
            AssignedDate = h.AssignedDate.AddTicks(-(h.AssignedDate.Ticks % TimeSpan.TicksPerSecond)),
            ReturnedDate = h.ReturnedDate?.AddTicks(-(h.ReturnedDate.Value.Ticks % TimeSpan.TicksPerSecond)),
            IsActive = h.IsActive,
            Notes = h.Notes
        }).ToList();
        return new PagedResult<AssetAssignmentHistoryDto>
        {
            Items = dtos,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ErrorOr<AssetResponseDto>> CreateAsync(
        CreateAssetDto dto,
        CancellationToken cancellationToken = default)
    {
        var deptResult = await _accessService.GetValidatedDepartmentIdAsync(cancellationToken);
        if (deptResult.IsError) return deptResult.Errors;
        if (deptResult.Value.HasValue)
        {
            dto.DepartmentId = deptResult.Value.Value;
        }
        return await CreateAssetInternalAsync(dto, cancellationToken);
    }

    public async Task<ErrorOr<AssetResponseDto>> CreateAssetByManagerAsync(
        CreateManagerAssetDto dto,
        CancellationToken cancellationToken = default)
    {
        var deptResult = await _accessService.GetValidatedDepartmentIdAsync(cancellationToken);
        if (deptResult.IsError) return deptResult.Errors;
        if (!deptResult.Value.HasValue)
        {
            return Error.Forbidden("Manager.NoDepartment", "Përdoruesi nuk është i caktuar në asnjë departament.");
        }
        var fullDto = new CreateAssetDto
        {
            AssetTag = dto.AssetTag,
            Name = dto.Name,
            Category = dto.Category.ToString(),
            SerialNumber = dto.SerialNumber,
            PurchaseValue = dto.PurchaseValue,
            PurchaseDate = dto.PurchaseDate,
            WarrantyExpirationDate = dto.WarrantyExpirationDate,
            IsShared = dto.IsShared,
            DepartmentId = deptResult.Value.Value
        };

        return await CreateAssetInternalAsync(fullDto, cancellationToken);
    }

    /// <summary>
    /// Metodë e brendshme për krijimin e asetit brenda një transaksioni DB dhe invalidimin e cache-it.
    /// </summary>
    private async Task<ErrorOr<AssetResponseDto>> CreateAssetInternalAsync(
        CreateAssetDto dto,
        CancellationToken cancellationToken)
    {
        if (dto == null) return Error.Validation("Asset.Null", "Të dhënat mungojnë.");
        if (dto.DepartmentId == Guid.Empty) return Error.Validation("Asset.DepartmentRequired", "Departamenti është i detyrueshëm.");

        if (!await _departmentsService.ExistsAsync(dto.DepartmentId, cancellationToken))
        {
            return Error.NotFound("Department.NotFound", "Departamenti nuk u gjet.");
        }
        if (!Enum.TryParse<AssetCategory>(dto.Category, true, out var category))
        {
            return Error.Validation("Asset.InvalidCategory", "Kategoria nuk është e vlefshme.");
        }
        var existing = await _assetRepository.GetByTagOrSerialAsync(dto.AssetTag, dto.SerialNumber, cancellationToken);
        if (existing != null)
        {
            return Error.Conflict("Asset.Exists", "AssetTag ose SerialNumber ekziston tashmë.");
        }
        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            AssetTag = dto.AssetTag,
            Name = dto.Name,
            Category = category,
            SerialNumber = dto.SerialNumber,
            PurchaseValue = dto.PurchaseValue,
            PurchaseDate = dto.PurchaseDate,
            WarrantyExpirationDate = dto.WarrantyExpirationDate,
            IsShared = dto.IsShared,
            DepartmentId = dto.DepartmentId,
            Status = AssetStatus.InStock,
        };
        // Fillimi i transaksionit për ruajtjen e sigurt të të dhënave
        await _transactionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            await _assetRepository.AddAsync(asset, cancellationToken);
            await _transactionRepository.CommitTransactionAsync(cancellationToken);
            // Pastrimi i cache-it pas ndryshimit
            await _cacheInvalidationService.InvalidateDashboardCacheAsync([asset.DepartmentId], cancellationToken);
            var createdAssetWithDept = await _assetRepository.GetByIdNoTrackingAsync(asset.Id, cancellationToken);
            return createdAssetWithDept!.AssetResponseDto();
        }
        catch (Exception ex)
        {
            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Gabim gjatë krijimit të asetit.");
            return Error.Failure("Database.Error", "Krijimi dështoi.");
        }
    }

    public async Task<ErrorOr<AssetResponseDto>> UpdateAsync(
        Guid id,
        UpdateAssetDto dto,
        CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetByTrackingIdAsync(id, cancellationToken);
        if (asset == null) return Error.NotFound("Asset.NotFound", "Aseti nuk u gjet.");
        var deptResult = await _accessService.GetValidatedDepartmentIdAsync(cancellationToken);
        if (deptResult.IsError) return deptResult.Errors;
        if (deptResult.Value.HasValue && asset.DepartmentId != deptResult.Value.Value)
        {
            return Error.Forbidden("Asset.Unauthorized", "Nuk keni leje të modifikoni asete jashtë departamentit tuaj.");
        }
        if (!Enum.TryParse<AssetCategory>(dto.Category, true, out var category))
        {
            return Error.Validation("Asset.InvalidCategory", "Kategoria nuk është e vlefshme.");
        }
        asset.Name = dto.Name;
        asset.Category = category;
        asset.PurchaseValue = dto.PurchaseValue;
        asset.WarrantyExpirationDate = dto.WarrantyExpirationDate;
        asset.IsShared = dto.IsShared;
        await _assetRepository.UpdateAsync(asset, cancellationToken);
        await _cacheInvalidationService.InvalidateDashboardCacheAsync([asset.DepartmentId], cancellationToken);
        return asset.AssetResponseDto();
    }

    public async Task<ErrorOr<Success>> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetByTrackingIdAsync(id, cancellationToken);
        if (asset == null) return Error.NotFound("Asset.NotFound", "Aseti nuk u gjet.");
        var deptResult = await _accessService.GetValidatedDepartmentIdAsync(cancellationToken);
        if (deptResult.IsError) return deptResult.Errors;
        if (deptResult.Value.HasValue && asset.DepartmentId != deptResult.Value.Value)
        {
            return Error.Forbidden("Asset.Unauthorized", "Nuk keni leje të fshini asete jashtë departamentit tuaj.");
        }
        if (asset.Status == AssetStatus.Assigned)
        {
            return Error.Conflict("Asset.Assigned", "Nuk mund të fshihet një aset që është aktualisht i caktuar.");
        }
        var assetDepartmentId = asset.DepartmentId;

        await _transactionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            await _assetRepository.DeleteAsync(asset, cancellationToken);
            await _transactionRepository.CommitTransactionAsync(cancellationToken);
            await _cacheInvalidationService.InvalidateDashboardCacheAsync([assetDepartmentId], cancellationToken);
            return Result.Success;
        }
        catch (Exception ex)
        {
            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Gabim gjatë fshirjes së asetit.");
            return Error.Failure("Database.Error", "Fshirja dështoi.");
        }
    }

    public async Task<ErrorOr<Success>> AssignAssetAsync(
        Guid assetId,
        AssignAssetDto dto,
        CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetByTrackingIdAsync(assetId, cancellationToken);
        if (asset == null) return Error.NotFound("Asset.NotFound", "Aseti nuk u gjet.");
        var deptResult = await _accessService.GetValidatedDepartmentIdAsync(cancellationToken);
        if (deptResult.IsError) return deptResult.Errors;
        // Validimi i përkatësisë së departamentit për caktim
        if (deptResult.Value.HasValue)
        {
            Guid userDeptId = deptResult.Value.Value;
            if (asset.DepartmentId != userDeptId)
            {
                return Error.Forbidden("Asset.Unauthorized", "Nuk mund të caktoni asete të një departamenti tjetër.");
            }
            bool isEmployeeInDept = await _departmentsService.IsEmployeeInDepartmentAsync(userDeptId, dto.EmployeeId, cancellationToken);
            if (!isEmployeeInDept)
            {
                return Error.Forbidden("Employee.Unauthorized", "Punonjësi nuk i përket departamentit tuaj.");
            }
        }
      var activeAssignments = asset.Assignments?.Where(a => a.IsActive).ToList() ?? [];
        bool alreadyAssignedToEmployee = activeAssignments.Any(a => a.EmployeeId == dto.EmployeeId);
        if (alreadyAssignedToEmployee)
        {
            return Error.Conflict(
                "Asset.AlreadyAssignedToEmployee",
                "Ky aset është tashmë i caktuar te ky punonjës.");
        }
        if (!asset.IsShared && activeAssignments.Count > 0)
        {
            return Error.Conflict(
                "Asset.AlreadyAssigned",
                "Kjo pajisje është e caktuar te një punonjës tjetër.");
        }
        await _transactionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            var assignment = new AssetAssignment
            {
                AssetId = assetId,
                EmployeeId = dto.EmployeeId,
                AssignedDate = DateTime.UtcNow,
                IsActive = true,
                Notes = dto.Notes
            };
            await _assetRepository.AddAssignmentAsync(assignment, cancellationToken);
            asset.Status = AssetStatus.Assigned;
            await _assetRepository.UpdateAsync(asset, cancellationToken);
            await _transactionRepository.CommitTransactionAsync(cancellationToken);
            await _cacheInvalidationService.InvalidateDashboardCacheAsync([asset.DepartmentId], cancellationToken);
            return Result.Success;
        }
        catch (Exception ex)
        {
            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Gabim gjatë caktimit të asetit.");
            return Error.Failure("Database.Error", "Caktimi dështoi.");
        }
    }

    public async Task<ErrorOr<Success>> UnassignAssetAsync(
        Guid assetId,
        UnassignAssetDto dto,
        CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetByTrackingIdAsync(assetId, cancellationToken);
        if (asset == null) return Error.NotFound("Asset.NotFound", "Aseti nuk u gjet.");
        var deptResult = await _accessService.GetValidatedDepartmentIdAsync(cancellationToken);
        if (deptResult.IsError) return deptResult.Errors;
        if (deptResult.Value.HasValue && asset.DepartmentId != deptResult.Value.Value)
        {
            return Error.Forbidden("Asset.Unauthorized", "Nuk mund të menaxhoni asete jashtë departamentit tuaj.");
        }
        var activeAssignment = await _assetRepository.GetActiveAssignmentAsync(assetId, dto.EmployeeId, cancellationToken);
        if (activeAssignment == null)
        {
            return Error.NotFound("Assignment.NotFound", "Nuk u gjet caktim aktiv.");
        }
        await _transactionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            activeAssignment.IsActive = false;
            activeAssignment.ReturnedDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(dto.Notes)) activeAssignment.Notes += $" | Return Note: {dto.Notes}";
            await _assetRepository.UpdateAssignmentAsync(activeAssignment, cancellationToken);
            bool hasOtherActive = asset.Assignments != null && asset.Assignments.Any(a => a.IsActive && a.Id != activeAssignment.Id);
            if (!hasOtherActive)
            {
                asset.Status = AssetStatus.InStock;
                await _assetRepository.UpdateAsync(asset, cancellationToken);
            }
            await _transactionRepository.CommitTransactionAsync(cancellationToken);
            await _cacheInvalidationService.InvalidateDashboardCacheAsync([asset.DepartmentId], cancellationToken);
            return Result.Success;
        }
        catch (Exception ex)
        {
            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Gabim gjatë lirimit të asetit.");
            return Error.Failure("Database.Error", "Lirimi dështoi.");
        }
    }

    public async Task<ErrorOr<Success>> UpdateDepartmentAsync(
        Guid assetId,
        Guid newDepartmentId,
        CancellationToken cancellationToken = default)
    {
        var asset = await _assetRepository.GetByTrackingIdAsync(assetId, cancellationToken);
        if (asset is null) return Error.NotFound("Asset.NotFound", "Aseti nuk u gjet.");
        var oldDeptId = asset.DepartmentId;
        if (oldDeptId == newDepartmentId) return Result.Success;
        if (!await _departmentsService.ExistsAsync(newDepartmentId, cancellationToken))
        {
            return Error.NotFound("Department.NotFound", "Departamenti i ri nuk u gjet.");
        }
        if (asset.Status == AssetStatus.Assigned)
        {
            return Error.Conflict("Asset.Assigned", "Nuk mund të ndryshohet departamenti i një aseti të caktuar.");
        }
        await _transactionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            asset.DepartmentId = newDepartmentId;
            await _assetRepository.UpdateAsync(asset, cancellationToken);
            await _transactionRepository.CommitTransactionAsync(cancellationToken);
            await _cacheInvalidationService.InvalidateDashboardCacheAsync([oldDeptId, newDepartmentId],cancellationToken);

            return Result.Success;
        }
        catch (Exception ex)
        {
            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Gabim gjatë ndryshimit të departamentit.");
            return Error.Failure("Database.Error", "Ndryshimi dështoi.");
        }
    }

    public async Task<ErrorOr<Success>> DeleteMultipleAsync(
        List<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids is null || ids.Count == 0) return Error.Validation("Asset.EmptyList", "Lista është e zbrazët.");
        var assets = await _assetRepository.GetByIdsWithTrackingAsync(ids, cancellationToken);
        if (assets.Count == 0) return Error.NotFound("Asset.NotFound", "Asetet nuk u gjetën.");
        var deptResult = await _accessService.GetValidatedDepartmentIdAsync(cancellationToken);
        if (deptResult.IsError) return deptResult.Errors;
        if (deptResult.Value.HasValue)
        {
            if (assets.Any(a => a.DepartmentId != deptResult.Value.Value))
            {
                return Error.Forbidden("Asset.Unauthorized", "Nuk keni leje të fshini asete jashtë departamentit.");
            }
        }
        if (assets.Any(a => a.Status == AssetStatus.Assigned))
        {
            return Error.Conflict("Asset.Assigned", "Një ose më shumë asete janë të caktuara.");
        }
        var departmentCounts = assets.GroupBy(a => a.DepartmentId).ToDictionary(g => g.Key, g => g.Count());
        var assetIdsToDelete = assets.Select(a => a.Id).ToList();

        await _transactionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            await _assetRepository.DeleteMultipleAsync(assetIdsToDelete, cancellationToken);
            await _transactionRepository.CommitTransactionAsync(cancellationToken);

            foreach (var departmentGroup in departmentCounts)
            {
                await _cacheInvalidationService.InvalidateDashboardCacheAsync([departmentGroup.Key], cancellationToken);
            }

            return Result.Success;
        }
        catch (Exception ex)
        {
            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Gabim gjatë fshirjes masive.");
            return Error.Failure("Database.Error", "Fshirja masive dështoi.");
        }
    }

    public async Task<ErrorOr<Success>> UpdateDepartmentMultipleAsync(
        List<Guid> assetIds,
        Guid newDepartmentId,
        CancellationToken cancellationToken = default)
    {
        if (assetIds is null || assetIds.Count == 0) return Error.Validation("Asset.EmptyList", "Lista është e zbrazët.");
        if (newDepartmentId == Guid.Empty) return Error.Validation("Department.InvalidId", "ID e pavlefshme.");

        if (!await _departmentsService.ExistsAsync(newDepartmentId, cancellationToken))
        {
            return Error.NotFound("Department.NotFound", "Departamenti nuk u gjet.");
        }

        var assets = await _assetRepository.GetByIdsWithTrackingAsync(assetIds, cancellationToken);
        if (assets.Any(a => a.Status == AssetStatus.Assigned))
        {
            return Error.Conflict("Asset.Assigned", "Asetet e caktuara nuk mund të lëvizen.");
        }

        var deptResult = await _accessService.GetValidatedDepartmentIdAsync(cancellationToken);
        if (deptResult.IsError) return deptResult.Errors;

        if (deptResult.Value.HasValue && assets.Any(a => a.DepartmentId != deptResult.Value.Value))
        {
            return Error.Forbidden("Asset.Unauthorized", "Nuk keni leje të lëvizni asete jashtë departamentit.");
        }

        var assetsToMove = assets.Where(a => a.DepartmentId != newDepartmentId).ToList();
        if (assetsToMove.Count == 0) return Result.Success;

        var moveIds = assetsToMove.Select(a => a.Id).ToList();
        var oldDepartmentIds = assetsToMove.Select(a => a.DepartmentId).Distinct().ToList();

        await _transactionRepository.BeginTransactionAsync(cancellationToken);
        try
        {
            await _assetRepository.UpdateDepartmentAsync(moveIds, newDepartmentId, cancellationToken);
            await _transactionRepository.CommitTransactionAsync(cancellationToken);

            foreach (var oldDeptId in oldDepartmentIds)
            {
                await _cacheInvalidationService.InvalidateDashboardCacheAsync([oldDeptId, newDepartmentId], cancellationToken);
            }
            await _cacheInvalidationService.InvalidateDashboardCacheAsync([newDepartmentId], cancellationToken);

            return Result.Success;
        }
        catch (Exception ex)
        {
            await _transactionRepository.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Gabim gjatë ndryshimit masiv të departamentit.");
            return Error.Failure("Database.Error", "Ndryshimi masiv dështoi.");
        }
    }

    public async Task<ErrorOr<Success>> UnassignAssetsForEmployeesAsync(
        List<Guid> employeeIds,
        CancellationToken cancellationToken = default)
    {
        if (employeeIds == null || employeeIds.Count == 0) return Result.Success;

        try
        {
            await _assetRepository.UnassignAssetsForEmployeesAsync(employeeIds, cancellationToken);
            return Result.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gabim gjatë lirimit të aseteve për punonjësit.");
            return Error.Failure("Asset.UnassignFailed", "Lirimi dështoi.");
        }
    }
}