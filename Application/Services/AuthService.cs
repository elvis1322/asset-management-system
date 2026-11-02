using Application.Models;
using Application.Interfaces;
using Domain.Entities;
using Domain.IRepository;
using ErrorOr;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Application.DTO.Auth;
using Application.Mappers;
namespace Application.Services;

public class AuthService(
    IUserAccountService userAccountService,
    IAccessService _accessService,
    UserManager<User> userManager,
    IRefreshTokenRepository refreshTokenRepository,
    ITokenService tokenService,
    ICacheService cacheService,
    IDashboardWarmupQueue warmupQueue,
    IEmployeeRepository employeeRepository,
    ILogger<AuthService> logger) : IAuthService
{

    public async Task<ErrorOr<UserResponseDto>> RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken = default)
    {
        var existingUser = await userAccountService.GetUserByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            logger.LogWarning("Tentativë regjistrimi me email ekzistues: {Email}", dto.Email);
            return Error.Conflict(code: "Auth.EmailExists", description: "Përdoruesi me këtë email ekzistues tashmë.");
        }
        if (string.IsNullOrWhiteSpace(dto.FirstName) ||
            string.IsNullOrWhiteSpace(dto.LastName) ||
            string.IsNullOrWhiteSpace(dto.Gender))
        {
            return Error.Validation(
                code: "Auth.InvalidData",
                description: "Emri, mbiemri dhe gjinia nuk mund të jenë të zbrazëta."
            );
        }
        var createResult = await userAccountService.CreateUserAsync(
            dto.Email,
            dto.Password,
            dto.FirstName,
            dto.LastName,
            dto.Gender.ToUpper(),
            "Client"
        );
        if (createResult.IsError)
        {
            logger.LogWarning("Dështoi krijimi i llogarisë për {Email}", dto.Email);
            return createResult.Errors;
        }
        var user = createResult.Value;
        user.CreatedBy = $"{dto.FirstName} {dto.LastName}";
        user.CreatedAt = DateTime.UtcNow;
        var roles = await userAccountService.GetUserRolesListAsync(user.Id);
        logger.LogInformation("U regjistrua me sukses përdoruesi {UserId} ({Email}).", user.Id, user.Email);
        return user.ToResponseDto(roles);
    }

    public async Task<ErrorOr<UserResponseLoginDto>> LoginAsync(UserLoginDto dto, CancellationToken cancellationToken = default)
    {
        var authResult = await userAccountService.VerifyCredentialsAsync(dto.Email, dto.Password);
        if (authResult.IsError)
        {
            logger.LogWarning("Tentativë e dështuar login për email: {Email}", dto.Email);
            return authResult.Errors;
        }
        var user = authResult.Value;
        var roles = await userAccountService.GetUserRolesListAsync(user.Id);
        var accessToken = tokenService.GenerateJwtToken(user, roles);
        var refreshTokenValue = tokenService.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            UserToken = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        var isAdmin = roles.Contains("Admin");
        var targetRole = isAdmin ? "Admin" : (roles.FirstOrDefault() ?? "Employee");
        Guid? deptId = null;
        if (!isAdmin)
        {
            var employee = await employeeRepository.GetByUserIdNoTrackingAsync(user.Id, cancellationToken);
            deptId = employee?.DepartmentId;
        }
        logger.LogInformation("Përdoruesi {UserId} u kyç me sukses (Role: {Role}, DeptId: {DeptId}).", user.Id, targetRole, deptId);
        await warmupQueue.EnqueueWarmupAsync(deptId, targetRole, cancellationToken);
        return user.ToResponseLoginDto(roles, accessToken, refreshTokenValue);
    }

    public async Task<ErrorOr<bool>> LogoutAsync(string refreshToken,CancellationToken cancellationToken = default)
    {
        var userIdResult = _accessService.GetCurrentUserId();

        if (userIdResult.IsError)
        {
            // ErrorOr lejon kthimin e gabimeve direkt
            return userIdResult.Errors;
        }
        var userId = userIdResult.Value;
        var user = await userAccountService.GetUserByIdAsync(userId);
        if (user == null)
        {
            logger.LogWarning("Tentativë logout për User ID të panjohur: {UserId}", userId);
            return Error.NotFound(code: "Auth.UserNotFound", description: "Përdoruesi nuk u gjet.");
        }
        var userTokens = await refreshTokenRepository.GetAllByUserIdAsync(userId, cancellationToken);
        var targetToken = userTokens.FirstOrDefault(t => t.UserToken == refreshToken && !t.IsRevoked);
        if (targetToken != null)
        {
            targetToken.IsRevoked = true;
            await refreshTokenRepository.UpdateAsync(targetToken, cancellationToken);
            string revokedTokenKey = CacheKeys.RevokedRefreshToken(refreshToken);
            await cacheService.SetAsync(revokedTokenKey, true, TimeSpan.FromDays(7), cancellationToken);
        }
        await userManager.UpdateSecurityStampAsync(user);
        logger.LogInformation("Përdoruesi {UserId} bëri logout me sukses.", userId);
        return true;
    }

    public async Task<ErrorOr<UserResponseLoginDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Error.Validation("Auth.InvalidToken", "Refresh token nuk mund të jetë bosh.");
        }
        string blacklistKey = CacheKeys.RevokedRefreshToken(refreshToken);
        var isBlacklisted = await cacheService.GetAsync<bool>(blacklistKey, cancellationToken);
        if (isBlacklisted)
        {
            logger.LogWarning("Tentativë rifreskimi me një Refresh Token të revokuar/blacklisted: {Token}", refreshToken);
            return Error.Unauthorized("Auth.TokenRevoked", "Ky refresh token është revokuar dhe nuk është më i vlefshëm.");
        }
        var tokenRecord = await refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
        if (tokenRecord is null)
        {
            logger.LogWarning("Refresh token nuk u gjet në databazë: {Token}", refreshToken);
            return Error.Unauthorized("Auth.InvalidToken", "Refresh token i pavlefshëm.");
        }
        if (tokenRecord.IsRevoked)
        {
            await cacheService.SetAsync(blacklistKey, true, TimeSpan.FromDays(7), cancellationToken);
            logger.LogWarning("Refresh token është i shënuar si i revokuar në DB: {Token}", refreshToken);
            return Error.Unauthorized("Auth.TokenRevoked", "Ky refresh token është revokuar.");
        }
        if (tokenRecord.IsUsed)
        {
            logger.LogWarning("Rrezik sigurie: Përdorim i përsëritur i një token-i tashmë të konsumuar (Token Reuse Detected)! User ID: {UserId}", tokenRecord.UserId);
            var allTokens = await refreshTokenRepository.GetAllByUserIdAsync(tokenRecord.UserId, cancellationToken);
            foreach (var t in allTokens.Where(t => !t.IsRevoked))
            {
                t.IsRevoked = true;
                await refreshTokenRepository.UpdateAsync(t, cancellationToken);
                await cacheService.SetAsync(CacheKeys.RevokedRefreshToken(t.UserToken), true, TimeSpan.FromDays(7), cancellationToken);
            }
            return Error.Unauthorized("Auth.TokenCompromised", "Zbuluar ripërdorim i paautorizuar i token-it. Të gjitha sesionet u mbyllën për siguri.");
        }
        if (tokenRecord.ExpiresAt <= DateTime.UtcNow)
        {
            logger.LogWarning("Refresh token ka skaduar: {Token}", refreshToken);
            return Error.Unauthorized("Auth.TokenExpired", "Refresh token ka skaduar. Ju lutem kyçuni përsëri.");
        }
        var user = await userAccountService.GetUserByIdAsync(tokenRecord.UserId);
        if (user is null || !user.IsActive)
        {
            logger.LogWarning("Përdoruesi për token-in {Token} nuk ekziston ose është joaktiv.", refreshToken);
            return Error.Unauthorized("Auth.UserNotFound", "Përdoruesi nuk ekziston ose llogaria është joaktive.");
        }
        tokenRecord.IsUsed = true;
        tokenRecord.IsRevoked = true;
        await refreshTokenRepository.UpdateAsync(tokenRecord, cancellationToken);
        await cacheService.SetAsync(blacklistKey, true, TimeSpan.FromDays(7), cancellationToken);
        var roles = await userAccountService.GetUserRolesListAsync(user.Id);
        var newAccessToken = tokenService.GenerateJwtToken(user, roles);
        var newRefreshTokenValue = tokenService.GenerateRefreshToken();
        var newRefreshTokenRecord = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            UserToken = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };
        await refreshTokenRepository.AddAsync(newRefreshTokenRecord, cancellationToken);
        logger.LogInformation("U krye me sukses Refresh Token Rotation për User ID: {UserId}.", user.Id);
        return user.ToResponseLoginDto(roles, newAccessToken, newRefreshTokenValue);
    }
}