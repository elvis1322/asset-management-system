using API.Security.RateLimiting;
using Application.DTO.Employees;
using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace API.Controllers;

[Authorize]
public class EmployeesController : BaseApiController
{
    private readonly IEmployeeService _employeeService;
    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }
    /// <summary>
    /// Merr listën e punonjësve në formë të faqerosur (paginated) me opsion filtrimi sipas termit.
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("GetAll-Employees")]
    [EnableRateLimiting(RateLimitConfiguration.ReadPolicy)]
    public async Task<ActionResult<PagedResult<EmployeeResponseDto>>> GetEmployees([FromQuery] PaginationParams paginationParams, [FromQuery] string? term, CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetEmployeesPagedAsync(paginationParams, term, cancellationToken);
        return HandleResult(result, "Punonjësit u listuan me sukses.");
    }
    /// <summary>
    /// Merr punonjësit e një departamenti specifik në formë të faqerosur.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("GetEmployeesByDepartment/{departmentId}")]
    [EnableRateLimiting(RateLimitConfiguration.ReadPolicy)]
    public async Task<ActionResult<PagedResult<EmployeeResponseDto>>> GetEmployeesByDepartment(Guid departmentId, [FromQuery] PaginationParams paginationParams, CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetEmployeesByDepartmentAsync(departmentId, paginationParams, cancellationToken);
        return HandleResult(result, "Punonjësit e departamentit u listuan me sukses.");
    }
    /// <summary>
    /// Merr detajet e një punonjësi specifik bazuar në ID-në e tij.
    /// </summary>
    [Authorize(Roles = "Admin,Manager")]
    [HttpGet("GetEmployee-ById")]
    [EnableRateLimiting(RateLimitConfiguration.ReadPolicy)]
    public async Task<IActionResult> GetEmployeeById(Guid employeeId, CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetEmployeeByIdAsync(employeeId, cancellationToken);
        return HandleResult(result);
    }
    /// <summary>
    /// Merr profilin e punonjësit bazuar në ID-në e përdoruesit (User ID).
    /// </summary>
    [HttpGet("GetByUserId/{userId:guid}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.ReadPolicy)]
    public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await _employeeService.GetByUserIdAsync(userId, cancellationToken);
        return HandleResult(result, "Profil i punonjësit për përdoruesin e kërkuar u mor me sukses.");
    }
    /// <summary>
    /// Krijon një punonjës të ri në sistem.
    /// </summary>
    [HttpPost("Create-Employee")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _employeeService.CreateAsync(dto, cancellationToken);
        return HandleResult(result, "Punonjësi u krijua me sukses.");
    }
    /// <summary>
    /// Përditëson të dhënat e një punonjësi ekzistues.
    /// </summary>
    [HttpPut("Update-Employee/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _employeeService.UpdateAsync(id, dto, cancellationToken);
        return HandleResult(result, "Të dhënat e punonjësit u përditësuan me sukses.", StatusCodes.Status200OK);
    }
    /// <summary>
    /// Përditëson departamentin e një punonjësi të vetëm.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPatch("Update-Department/{id:guid}")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] UpdateEmployeeDepartmentDto request, CancellationToken cancellationToken)
    {
        var result = await _employeeService.UpdateDepartmentAsync(id, request.NewDepartmentId, cancellationToken);
        return HandleResult(result, "Punonjësi u bë update departamenti me sukses.", StatusCodes.Status200OK);
    }
    /// <summary>
    /// Fshin një punonjës nga sistemi bazuar në ID-në e tij.
    /// </summary>
    [HttpDelete("Delete-Employee/{id:guid}")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _employeeService.DeleteAsync(id, cancellationToken);
        return HandleResult(result, "Punonjësi u fshi me sukses.");
    }
    /// <summary>
    /// Fshin në mënyrë masive (bulk delete) një listë punonjësish përmes ID-ve të tyre.
    /// </summary>
    [HttpPost("Bulk-Delete")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> BulkDelete([FromBody] List<Guid> employeeIds, CancellationToken cancellationToken)
    {
        var result = await _employeeService.DeleteMultipleAsync(employeeIds, cancellationToken);
        return HandleResult(result, "Punonjësit u fshinë me sukses", StatusCodes.Status204NoContent);
    }
    /// <summary>
    /// Përditëson në mënyrë masive departamentin për një listë punonjësish të zgjedhur.
    /// </summary>
    [HttpPut("Bulk-Update-Department")]
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> BulkUpdateDepartment([FromBody] BulkUpdateEDepartmentDto dto, CancellationToken cancellationToken)
    {
        var result = await _employeeService.UpdateDepartmentMultipleAsync(dto.EmployeeIds, dto.NewDepartmentId, cancellationToken);
        return HandleResult(result, "Departamenti i punonjësve u përditësua me sukses.", StatusCodes.Status200OK);
    }
    /// <summary>
    /// Promovon një klient/përdorues ekzistues në rol punonjësi në sistem.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{userId:guid}/Promote-Client-To-Employee")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> PromoteUserToEmployee(Guid userId, [FromBody] PromoteUserDto dto, CancellationToken cancellationToken)
    {
        var result = await _employeeService.PromoteUserToEmployeeAsync(userId, dto, cancellationToken);
        return HandleResult(result, "Punonjësi u promovua me sukses.", StatusCodes.Status200OK);
    }
    /// <summary>
    /// Përditëson rolin e një përdoruesi të lidhur me punonjësin.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{employeeId:guid}/UpdateRole")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> UpdateUserRole(
        Guid employeeId,
        [FromBody] UpdateUserRoleDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _employeeService.UpdateUserRoleAsync(employeeId, dto, cancellationToken);
        return HandleResult(result, "Roli u përditësua me sukses.", StatusCodes.Status200OK);
    }
}