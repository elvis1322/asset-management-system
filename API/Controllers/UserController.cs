using API.Security;
using API.Security.RateLimiting;
using Application.DTO.Admin;
using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace API.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
    /// <summary>
    /// Merr listën e përdoruesve në formë të faqerosur (paginated) me opsion filtrimi sipas termit.
    /// </summary>

    [HttpGet("GetAll-Users")]
    [EnableRateLimiting(RateLimitConfiguration.ReadPolicy)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams paginationParams, [FromQuery] string? term, CancellationToken cancellationToken)
    {
        var result = await _userService.GetUsersPagedAsync(paginationParams, term, cancellationToken);
        return HandleResult(result, "Lista e përdoruesve u mor me sukses.");
    }
    /// <summary>
    /// Merr detajet e një përdoruesi specifik bazuar në ID-në e tij.
    /// </summary>
    [HttpGet("GetUserById/{id:guid}")]
    [EnableRateLimiting(RateLimitConfiguration.ReadPolicy)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _userService.GetByIdAsync(id, cancellationToken);
        return HandleResult(result, "Të dhënat e përdoruesit u morën me sukses.");
    }
    /// <summary>
    /// Krijon një klient/përdorues të ri në sistem nga ana e Administratorit.
    /// </summary>
    [HttpPost("Create-Client")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> Create([FromBody] CreateClientDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _userService.CreateAsync(dto, cancellationToken);
        return HandleResult(result, "Përdoruesi u krijua me sukses.");
    }
    /// <summary>
    /// Përditëson të dhënat e një përdoruesi ekzistues.
    /// </summary>
    [HttpPut("UpdateUser/{id:guid}")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAdminUserDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _userService.UpdateAsync(id, dto, cancellationToken);
        return HandleResult(result, "Të dhënat e përdoruesit u përditësuan me sukses.", StatusCodes.Status200OK);
    }
    /// <summary>
    /// Përditëson pjesërisht (patch) të dhënat e një përdoruesi në sistem.
    /// </summary>
    [HttpPatch("PatchUser/{id:guid}")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> Patch(Guid id, [FromBody] PatchAdminUserDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _userService.PatchAsync(id, dto, cancellationToken);
        return HandleResult(result, "Përdoruesi u modifikua me sukses.", StatusCodes.Status200OK);
    }
    /// <summary>
    /// Fshin një përdorues nga sistemi bazuar në ID-në e tij.
    /// </summary>
    [HttpDelete("DeleteUser/{id:guid}")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _userService.DeleteAsync(id, cancellationToken);
        return HandleResult(result, "Përdoruesi u fshi me sukses.");
    }
}