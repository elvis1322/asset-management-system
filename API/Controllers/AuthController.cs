using API.Security.RateLimiting;
using Application.DTO.Auth;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
namespace API.Controllers;

public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    /// <summary>
    /// Regjistron një përdorues të ri në sistem.
    /// </summary>
    [HttpPost("Register")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitConfiguration.RegisterPolicy)]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _authService.RegisterAsync(dto, cancellationToken);
        return HandleResult(result, "Regjistrimi u krye me sukses.");
    }
    /// <summary>
    /// Mundëson kyçjen (login) e përdoruesit dhe kthen token-in e qasjes.
    /// </summary>
    [HttpPost("Login")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitConfiguration.LoginPolicy)]
    public async Task<IActionResult> Login([FromBody] UserLoginDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _authService.LoginAsync(dto, cancellationToken);
        return HandleResult(result, "Kyçja u krye me sukses.", StatusCodes.Status200OK);
    }
    /// <summary>
    /// Çvalidon refresh token-in ekzistues dhe e nxjerr përdoruesin jashtë llogarisë (logout).
    /// </summary>
    [HttpPost("Logout")]
    [Authorize]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequestDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _authService.LogoutAsync(dto.RefreshToken, cancellationToken);
        return HandleResult(result, "Dalja nga llogaria u krye me sukses.", StatusCodes.Status200OK);
    }
    /// <summary>
    /// Rifreskon token-in e skaduar duke përdorur Refresh Token.
    /// </summary>
    [HttpPost("Refresh-Token")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitConfiguration.SensitivePolicy)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken, cancellationToken);
        return HandleResult(result, "Token u rifreskua me sukses.", StatusCodes.Status200OK);
    }
}