using API.Security.RateLimiting;
using Application.DTO.Profile;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace API.Controllers;

[Authorize]
public class ProfileController : BaseApiController
{
    private readonly IProfileService _profileService;
    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }
    /// <summary>
    /// Merr të dhënat e profilit të përdoruesit të loguar aktualisht.
    /// </summary>
    [HttpGet("Get-Profile")]
    [EnableRateLimiting(RateLimitConfiguration.ReadPolicy)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken = default)
    {
        var result = await _profileService.GetProfileAsync(cancellationToken);
        return HandleResult(result, "Të dhënat e profilit u morën me sukses.");
    }
    /// <summary>
    /// Përditëson të dhënat e profilit të përdoruesit të loguar.
    /// </summary>
    [HttpPut("Update-Profile")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _profileService.UpdateProfileAsync(dto, cancellationToken);
        return HandleResult(result, "Profili u përditësua me sukses.", StatusCodes.Status200OK);
    }
    /// <summary>
    /// Ndryshon fjalëkalimin e përdoruesit të loguar.
    /// </summary>
    [HttpPost("Change-Password")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _profileService.ChangePasswordAsync(dto, cancellationToken);
        return HandleResult(result, "Fjalëkalimi u ndryshua me sukses.", StatusCodes.Status200OK);
    }
    /// <summary>
    /// Kërkon rivendosjen e fjalëkalimit duke dërguar udhëzimet në email.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("Forgot-Password")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _profileService.ForgotPasswordAsync(dto, cancellationToken);
        return HandleResult(result, "Udhëzimet për rivendosjen e fjalëkalimit u dërguan në email.");
    }
    /// <summary>
    /// Rivendos fjalëkalimin e ri duke përdorur token-in e dërguar.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("Reset-Password")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _profileService.ResetPasswordAsync(dto, cancellationToken);
        return HandleResult(result, "Fjalëkalimi u rivendos me sukses.");
    }
    /// <summary>
    /// Nis kërkesën për ndryshimin e adresës së email-it të përdoruesit.
    /// </summary>
    [HttpPost("Request-Email-Change")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> RequestEmailChange([FromBody] RequestEmailChangeDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _profileService.RequestEmailChangeAsync(dto, cancellationToken);
        return HandleResult(result, "Kërkesa për ndryshimin e email-it u dërgua me sukses.");
    }
    /// <summary>
    /// Konfirmon dhe përfundon ndryshimin e email-it përmes kodit ose token-it.
    /// </summary>
    [HttpPost("Confirm-Email-Change")]
    [EnableRateLimiting(RateLimitConfiguration.WritePolicy)]
    public async Task<IActionResult> ConfirmEmailChange([FromBody] ConfirmEmailChangeDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _profileService.ConfirmEmailChangeAsync(dto, cancellationToken);
        return HandleResult(result, "Email-i u ndryshua me sukses.");
    }
}