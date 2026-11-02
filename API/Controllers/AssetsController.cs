using API.Security.RateLimiting;
using Application.DTO.Assets;
using Microsoft.AspNetCore.RateLimiting;
using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace API.Controllers;

[Authorize]

public class AssetsController : BaseApiController
{
    private readonly IAssetsService _assetService;
    public AssetsController(IAssetsService assetService)
    {
        _assetService = assetService;
    }
    /// <summary>
    /// Merr listën e pajisjeve në formë të faqerosur (paginated) me opsione filtrimi sipas termit, statusit dhe kategorisë.
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("GetAll-Assets")]
    [EnableRateLimiting(RateLimitConfiguration.ReadPolicy)]
    public async Task<ActionResult<PagedResult<AssetResponseDto>>> GetAssets([FromQuery] PaginationParams paginationParams, [FromQuery] string? term, [FromQuery] AssetStatus? status, [FromQuery] AssetCategory? category, CancellationToken cancellationToken)
    {
        var result = await _assetService.GetAssetsPagedAsync(paginationParams, term, status, category, cancellationToken);
        return HandleResult(result, "Pajisjet u morën me sukses.");
    }
    /// <summary>
    /// Kthen pajisjet e caktuara për përdoruesin e loguar aktualisht.
    /// </summary>
    [HttpGet("Get-My-Assets")]
    [EnableRateLimiting(RateLimitConfiguration.ReadPolicy)]
    public async Task<IActionResult> GetMyAssets(CancellationToken cancellationToken = default)
    {
        var result = await _assetService.GetMyAssetsAsync(cancellationToken);
        return HandleResult(result, "Pajisjet tuaja u morën me sukses.");
    }
    /// <summary>
    /// Merr asetet që i përkasin një departamenti specifik sipas ID-së së departamentit.
    /// </summary>
    [HttpGet("Get-AssetsByDepartmentId/{departmentId:guid}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.ReadPolicy)]
    public async Task<IActionResult> GetByDepartment(Guid departmentId, [FromQuery] PaginationParams paginationParams, [FromQuery] AssetStatus? status, [FromQuery] AssetCategory? category, CancellationToken cancellationToken = default)
    {
        var result = await _assetService.GetAssetsByDepartmentAsync(departmentId, paginationParams, status, category, cancellationToken);
        return HandleResult(result, "Asetet e departamentit u morën me sukses.");
    }
    /// <summary>
    /// Merr asetet e lidhura me një punonjës specifik sipas ID-së së punonjësit.
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("Get-EmployeeAssets/{employeeId:guid}")]
    [EnableRateLimiting(RateLimitConfiguration.ReadPolicy)]
    public async Task<IActionResult> GetEmployeeAssets(Guid employeeId, [FromQuery] PaginationParams paginationParams, CancellationToken cancellationToken = default)
    {
        var result = await _assetService.GetAssetsByEmployeeAsync(employeeId, paginationParams, cancellationToken);
        return HandleResult(result, "Të dhënat e pajisjeve të punonjësit u morën me sukses.");
    }
    /// <summary>
    /// Merr detajet e një pajisjeje specifike bazuar në ID-në e saj.
    /// </summary>
    [HttpGet("Get-AssetById/{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    [EnableRateLimiting(RateLimitConfiguration.ReadPolicy)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _assetService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result, "Të dhënat e pajisjes u morën me sukses.");
    }
    /// <summary>
    /// Kthen historikun e lëvizjeve dhe ndryshimeve për një pajisje të caktuar.
    /// </summary>
    [HttpGet("Get-Asset-History/{assetId:guid}")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    [EnableRateLimiting(RateLimitConfiguration.ReadPolicy)]
    public async Task<IActionResult> GetHistory(Guid assetId, [FromQuery] PaginationParams pagination, CancellationToken cancellationToken = default)
    {
        var result = await _assetService.GetHistoryAsync(assetId, pagination, cancellationToken);
        return HandleResult(result, "Historiku i pajisjes u mor me sukses.");
    }
    /// <summary>
    /// Regjistron një pajisje të re në sistem nga ana e Administratorit.
    /// </summary>
    [HttpPost("Create-By-Admin")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> CreateByAdmin([FromBody] CreateAssetDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _assetService.CreateAsync(dto, cancellationToken);
        return HandleResult(result, "Pajisja u regjistrua me sukses.");
    }
    /// <summary>
    /// Regjistron një pajisje të re në sistem nga ana e Menaxherit.
    /// </summary>
    [HttpPost("Create-By-Manager")]
    [Authorize(Roles = "Manager")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> CreateByManager([FromBody] CreateManagerAssetDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _assetService.CreateAssetByManagerAsync(dto, cancellationToken);
        return HandleResult(result, "Pajisja u regjistrua me sukses.");
    }
    /// <summary>
    /// I dorëzon/cakton një pajisje një punonjësi.
    /// </summary>
    [HttpPost("Assign-Asset/{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignAssetDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _assetService.AssignAssetAsync(id, dto, cancellationToken);
        return HandleResult(result, "Pajisja iu dorëzua punonjësit me sukses.");
    }
    /// <summary>
    /// Kthen pajisjen mbrapsht nga punonjësi (unassign).
    /// </summary>
    [HttpPost("Unassign-Asset/{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> Unassign(Guid id, [FromBody] UnassignAssetDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _assetService.UnassignAssetAsync(id, dto, cancellationToken);
        return HandleResult(result, "Pajisja u kthye nga punonjësi me sukses.");
    }
    /// <summary>
    /// Përditëson të dhënat e një pajisjeje ekzistuese.
    /// </summary>
    [HttpPut("Update-AssetById/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAssetDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _assetService.UpdateAsync(id, dto, cancellationToken);
        return HandleResult(result, "Të dhënat e pajisjes u përditësuan me sukses.", StatusCodes.Status200OK);
    }
    /// <summary>
    /// Përditëson departamentin e një pajisjeje të vetme.
    /// </summary>
    [HttpPatch("Update-Asset-DepartmentById/{id:guid}/department")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateAssetDepartmentDto request, CancellationToken cancellationToken)
    {
        var result = await _assetService.UpdateDepartmentAsync(id, request.NewDepartmentId, cancellationToken);
        return HandleResult(result, "Asseti u bë update departamenti me sukses.", StatusCodes.Status200OK);
    }
    /// <summary>
    /// Fshin një pajisje nga sistemi bazuar në ID-në e saj.
    /// </summary>
    [HttpDelete("Delete-AssetById/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _assetService.DeleteAsync(id, cancellationToken);
        return HandleResult(result, "Pajisja u fshi me sukses.");
    }
    /// <summary>
    /// Fshin në mënyrë masive (bulk delete) një listë asetesh përmes ID-ve të tyre.
    /// </summary>
    [HttpPost("Bulk-Delete")]
    [Authorize(Roles = "Admin")]    
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> DeleteMultipleAsync([FromBody] List<Guid> ids, CancellationToken cancellationToken)
    {
        var result = await _assetService.DeleteMultipleAsync(ids, cancellationToken);
        return HandleResult(result, "Asetet u fshinë me sukses.", StatusCodes.Status200OK);
    }
    /// <summary>
    /// Përditëson në mënyrë masive departamentin për një listë asetesh të zgjedhura.
    /// </summary>
    [HttpPut("Bulk-Update-Department")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> UpdateDepartmentMultipleAsync([FromBody] BulkUpdateDepartmentDto dto, CancellationToken cancellationToken)
    {
        var result = await _assetService.UpdateDepartmentMultipleAsync(dto.AssetIds, dto.NewDepartmentId, cancellationToken);
        return HandleResult(result, "Departamenti u ndryshua me sukses për asetet e zgjedhura.", StatusCodes.Status200OK);
    }
}