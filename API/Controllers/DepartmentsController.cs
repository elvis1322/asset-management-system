using API.Security.RateLimiting;
using Application.DTO.Departments;
using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace API.Controllers;

[Authorize]
public class DepartmentsController : BaseApiController
{
    private readonly IDepartmentsService _departmentService;
    public DepartmentsController(IDepartmentsService departmentService)
    {
        _departmentService = departmentService;
    }
    /// <summary>
    /// Merr listën e departamenteve në formë të faqerosur (paginated) me opsion filtrimi sipas termit.
    /// </summary>
    [HttpGet("GetAll-Departments")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.ReadPolicy)]
    public async Task<IActionResult> GetPaged([FromQuery] PaginationParams paginationParams, [FromQuery] string? term, CancellationToken cancellationToken = default)
    {
        var result = await _departmentService.GetPagedAsync(paginationParams, term, cancellationToken);
        return HandleResult(result, "Lista e departamenteve u mor me sukses.");
    }
    /// <summary>
    /// Merr detajet e një departamenti specifik bazuar në ID-në e tij.
    /// </summary>
    [HttpGet("Get-DepartmentById/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.ReadPolicy)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _departmentService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result, "Të dhënat e departamentit u morën me sukses.");
    }
    /// <summary>
    /// Krijon një departament të ri në sistem nga ana e Administratorit.
    /// </summary>
    [HttpPost("Create-Department")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _departmentService.CreateAsync(dto, cancellationToken);
        return HandleResult(result, "Departamenti u krijua me sukses.");
    }
    /// <summary>
    /// Përditëson të dhënat e një departamenti nga ana e Administratorit.
    /// </summary>
    [HttpPut("Update-Department/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> UpdateByAdmin(Guid id, [FromBody] UpdateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _departmentService.UpdateAsync(id, dto, cancellationToken);
        return HandleResult(result, "Departamenti u përditësua me sukses.", StatusCodes.Status200OK);
    }
    /// <summary>
    /// Fshin një departament nga sistemi bazuar në ID-në e tij.
    /// </summary>
    [HttpDelete("Delete-Department/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _departmentService.DeleteAsync(id, cancellationToken);
        return HandleResult(result, "Departamenti u fshi me sukses.");
    }
    /// <summary>
    /// Përditëson departamentin e menaxherit të loguar aktualisht.
    /// </summary>
    [HttpPut("Update-My-Department")]
    [Authorize(Roles = "Manager")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> UpdateByManager([FromBody] UpdateDepartmentDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _departmentService.UpdateByManagerAsync(dto, cancellationToken);
        return HandleResult(result, "Departamenti juaj u përditësua me sukses.", StatusCodes.Status200OK);
    }
    /// <summary>
    /// Përditëson kodin e një departamenti specifik.
    /// </summary>
    [HttpPatch("Update-Department-Code/{id:guid}/code")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> UpdateDepartmentCode(Guid id, [FromBody] UpdateDepartamentCodeDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _departmentService.UpdateCodeAsync(id, dto, cancellationToken);
        return HandleResult(result, "Kodi i departamentit u përditësua me sukses.", StatusCodes.Status200OK);
    }
}