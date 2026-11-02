using Application.DTO.Profile;
using Application.Interfaces;
using Application.Mappers;
using Application.Models;
using Domain.IRepository;
using ErrorOr;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ProfileService : IProfileService
{
    private const int MaxForgotPasswordRequests = 3;
    private const int MaxEmailChangeRequests = 2;
    private static readonly TimeSpan ForgotPasswordWindow = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan EmailChangeWindow = TimeSpan.FromMinutes(30);

    private readonly IUserAccountService _userAccountService;
    private readonly IMemoryCache _memoryCache;
    private readonly ICacheService _cacheService;
    private readonly IEmailService _emailService;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<ProfileService> _logger;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAccessService _accessService;

    public ProfileService(
        IUserAccountService userAccountService,
        IMemoryCache memoryCache,
        ICacheService cacheService,
        IEmailService emailService,
        IEmployeeRepository employeeRepository,
        ITransactionRepository transactionRepository,
        ILogger<ProfileService> logger,
        IRefreshTokenRepository refreshTokenRepository,
        IAccessService accessService)
    {
        _userAccountService = userAccountService;
        _memoryCache = memoryCache;
        _cacheService = cacheService;
        _emailService = emailService;
        _employeeRepository = employeeRepository;
        _transactionRepository = transactionRepository;
        _logger = logger;
        _refreshTokenRepository = refreshTokenRepository;
        _accessService = accessService;
    }

    public async Task<ErrorOr<ProfileResponseDto>> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        var userIdResult = _accessService.GetCurrentUserId();
        if (userIdResult.IsError)
            return userIdResult.Errors;

        var userId = userIdResult.Value;
        var user = await _userAccountService.GetUserByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Profili nuk u gjet për UserId: {UserId}", userId);
            return Error.NotFound("User.NotFound", "Përdoruesi nuk u gjet.");
        }

        var roles = await _userAccountService.GetUserRolesListAsync(userId);
        return user.MapToProfileResponseDto(roles);
    }

    public async Task<ErrorOr<ProfileResponseDto>> UpdateProfileAsync(UpdateProfileDto dto, CancellationToken cancellationToken = default)
    {
        var userIdResult = _accessService.GetCurrentUserId();
        if (userIdResult.IsError)
            return userIdResult.Errors;

        var userId = userIdResult.Value;
        var user = await _userAccountService.GetUserByIdAsync(userId);
        if (user is null)
        {
            return Error.NotFound("User.NotFound", "Përdoruesi nuk u gjet.");
        }

        var transactionStarted = false;

        try
        {
            await _transactionRepository.BeginTransactionAsync(cancellationToken);
            transactionStarted = true;

            var identityResult = await _userAccountService.UpdateUserIdentityAsync(userId, dto.FirstName, dto.LastName, user.Email!, dto.Gender);
            if (identityResult.IsError)
            {
                await RollbackSafelyAsync();
                transactionStarted = false;
                _logger.LogWarning("Përditësimi i Identity dështoi për UserId: {UserId}", userId);
                return identityResult.Errors;
            }

            var employee = await _employeeRepository.GetByUserIdTrackingAsync(userId, cancellationToken);
            if (employee is not null)
            {
                employee.FirstName = dto.FirstName;
                employee.LastName = dto.LastName;
                employee.Gender = dto.Gender;
                await _employeeRepository.UpdateAsync(employee, cancellationToken);
            }

            await _transactionRepository.CommitTransactionAsync(cancellationToken);
            transactionStarted = false;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            if (transactionStarted)
                await RollbackSafelyAsync();
            throw;
        }
        catch (Exception ex)
        {
            if (transactionStarted)
                await RollbackSafelyAsync();

            _logger.LogError(ex, "Gabim gjatë përditësimit të profilit për UserId: {UserId}", userId);
            return Error.Failure("Profile.UpdateFailed", "Përditësimi i profilit dështoi.");
        }

        var updatedUser = await _userAccountService.GetUserByIdAsync(userId);
        if (updatedUser is null)
        {
            return Error.NotFound("User.NotFound", "Përdoruesi nuk u gjet pas përditësimit.");
        }

        var roles = await _userAccountService.GetUserRolesListAsync(userId);
        _logger.LogInformation("Profili u përditësua për UserId: {UserId}", userId);

        // Nuk kërkohet invalidim i dashboard-it.
        return updatedUser.MapToProfileResponseDto(roles);
    }

    // CHANGE PASSWORD
    public async Task<ErrorOr<bool>> ChangePasswordAsync(ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        var userIdResult = _accessService.GetCurrentUserId();
        if (userIdResult.IsError)
            return userIdResult.Errors;

        var userId = userIdResult.Value;
        var changeResult = await _userAccountService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);
        if (changeResult.IsError)
        {
            _logger.LogWarning("Ndryshimi i fjalëkalimit dështoi për UserId: {UserId}", userId);
            return changeResult.Errors;
        }

        try
        {
            await RevokeAllRefreshTokensAsync(userId, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fjalëkalimi u ndryshua, por revokimi i sesioneve dështoi për UserId: {UserId}", userId);
            return Error.Failure("Auth.SessionRevocationFailed", "Fjalëkalimi u ndryshua, por revokimi i të gjitha sesioneve nuk u përfundua plotësisht.");
        }

        _logger.LogInformation("Fjalëkalimi u ndryshua dhe sesionet u revokuan për UserId: {UserId}", userId);
        return true;
    }

    // FORGOT PASSWORD
    public async Task<ErrorOr<bool>> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var email = dto.Email.Trim();
        var normalizedEmail = email.ToUpperInvariant();
        var cacheKey = $"ForgotPasswordAttempts_{normalizedEmail}";

        if (IsRateLimitExceeded(cacheKey, MaxForgotPasswordRequests))
        {
            return Error.Custom(
                type: 429,
                code: "Auth.RateLimitExceeded",
                description: "Keni bërë shumë kërkesa. Ju lutem provoni përsëri pas 15 minutash.");
        }

        IncrementRateLimit(cacheKey, ForgotPasswordWindow);

        try
        {
            var token = await _userAccountService.GeneratePasswordResetTokenAsync(email);
            if (token is not null)
            {
                await _emailService.SendResetPasswordEmailAsync(email, token, cancellationToken);
                _logger.LogInformation("U dërgua email-i për resetimin e fjalëkalimit.");
            }
            else
            {
                // Mos zbulo nëse email-i ekziston.
                _logger.LogInformation("U procesua kërkesa për resetim fjalëkalimi.");
            }

            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dështoi procesimi i kërkesës për resetim fjalëkalimi.");
            return Error.Failure("Auth.PasswordResetRequestFailed", "Kërkesa nuk mund të procesohej. Ju lutem provoni përsëri.");
        }
    }

    // RESET PASSWORD
    public async Task<ErrorOr<bool>> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userAccountService.GetUserByEmailAsync(dto.Email);
        if (user is null)
        {
            return Error.Validation("Auth.ResetPasswordFailed", "Token-i është i pavlefshëm ose ka skaduar.");
        }

        var resetResult = await _userAccountService.ResetPasswordAsync(dto.Email, dto.Token, dto.NewPassword);
        if (resetResult.IsError)
        {
            _logger.LogWarning("Resetimi i fjalëkalimit dështoi për UserId: {UserId}", user.Id);
            return resetResult.Errors;
        }

        try
        {
            await RevokeAllRefreshTokensAsync(user.Id, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fjalëkalimi u resetua, por revokimi i sesioneve dështoi për UserId: {UserId}", user.Id);
            return Error.Failure("Auth.SessionRevocationFailed", "Fjalëkalimi u resetua, por revokimi i të gjitha sesioneve nuk u përfundua plotësisht.");
        }

        _logger.LogInformation("Fjalëkalimi u resetua dhe sesionet u revokuan për UserId: {UserId}", user.Id);
        return true;
    }

    // REQUEST EMAIL CHANGE
    public async Task<ErrorOr<bool>> RequestEmailChangeAsync(RequestEmailChangeDto dto, CancellationToken cancellationToken = default)
    {
        var userIdResult = _accessService.GetCurrentUserId();
        if (userIdResult.IsError)
            return userIdResult.Errors;

        var userId = userIdResult.Value;
        var newEmail = dto.NewEmail.Trim();
        var cacheKey = $"ChangeEmailAttempts_{userId}";

        if (IsRateLimitExceeded(cacheKey, MaxEmailChangeRequests))
        {
            return Error.Custom(
                type: 429,
                code: "Auth.RateLimitExceeded",
                description: "Keni kërkuar ndryshimin e email-it shumë herë. Ju lutem provoni përsëri pas 30 minutash.");
        }

        var tokenResult = await _userAccountService.GenerateEmailChangeTokenAsync(userId, newEmail);
        if (tokenResult.IsError)
            return tokenResult.Errors;

        IncrementRateLimit(cacheKey, EmailChangeWindow);

        try
        {
            await _emailService.SendEmailChangeConfirmationAsync(newEmail, tokenResult.Value, cancellationToken);
            _logger.LogInformation("U dërgua kërkesa për ndryshimin e email-it për UserId: {UserId}", userId);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dërgimi i email-it të konfirmimit dështoi për UserId: {UserId}", userId);
            return Error.Failure("Profile.EmailChangeEmailFailed", "Email-i i konfirmimit nuk mund të dërgohej. Ju lutem provoni përsëri.");
        }
    }

    // CONFIRM EMAIL CHANGE
    public async Task<ErrorOr<bool>> ConfirmEmailChangeAsync(ConfirmEmailChangeDto dto, CancellationToken cancellationToken = default)
    {
        var userIdResult = _accessService.GetCurrentUserId();
        if (userIdResult.IsError)
            return userIdResult.Errors;

        var userId = userIdResult.Value;
        var newEmail = dto.NewEmail.Trim();
        var transactionStarted = false;

        try
        {
            await _transactionRepository.BeginTransactionAsync(cancellationToken);
            transactionStarted = true;

            var confirmResult = await _userAccountService.ConfirmEmailChangeAsync(userId, newEmail, dto.Token);
            if (confirmResult.IsError)
            {
                await RollbackSafelyAsync();
                transactionStarted = false;
                return confirmResult.Errors;
            }

            var employee = await _employeeRepository.GetByUserIdTrackingAsync(userId, cancellationToken);
            if (employee is not null)
            {
                employee.Email = newEmail;
                await _employeeRepository.UpdateAsync(employee, cancellationToken);
            }

            await _transactionRepository.CommitTransactionAsync(cancellationToken);
            transactionStarted = false;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            if (transactionStarted)
                await RollbackSafelyAsync();
            throw;
        }
        catch (Exception ex)
        {
            if (transactionStarted)
                await RollbackSafelyAsync();

            _logger.LogError(ex, "Gabim gjatë konfirmimit të ndryshimit të email-it për UserId: {UserId}", userId);
            return Error.Failure("Profile.EmailChangeFailed", "Konfirmimi i ndryshimit të email-it dështoi.");
        }

        // Email-i nuk ndikon në statistikat e dashboard-it.
        _logger.LogInformation("Email-i u ndryshua për UserId: {UserId}", userId);
        return true;
    }

    // REFRESH TOKEN REVOCATION
    private async Task RevokeAllRefreshTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userTokens = await _refreshTokenRepository.GetAllByUserIdAsync(userId, cancellationToken);
        var activeTokens = userTokens
            .Where(token => !token.IsRevoked)
            .ToList();

        // Ruaj revokimin në databazë.
        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            await _refreshTokenRepository.UpdateAsync(token, cancellationToken);
        }

        // Blacklist vetëm token-at që ende nuk kanë skaduar.
        var now = DateTime.UtcNow;
        foreach (var token in activeTokens)
        {
            var remainingTime = token.ExpiresAt - now;
            if (remainingTime <= TimeSpan.Zero)
                continue;

            await _cacheService.SetAsync(CacheKeys.RevokedRefreshToken(token.UserToken), "revoked", remainingTime, cancellationToken);
        }
    }

    // RATE LIMIT HELPERS
    private bool IsRateLimitExceeded(string cacheKey, int maximumRequests)
    {
        return _memoryCache.TryGetValue(cacheKey, out int requests) && requests >= maximumRequests;
    }

    private void IncrementRateLimit(string cacheKey, TimeSpan window)
    {
        _memoryCache.TryGetValue(cacheKey, out int requests);
        _memoryCache.Set(cacheKey, requests + 1, window);
    }

    private async Task RollbackSafelyAsync()
    {
        try
        {
            await _transactionRepository.RollbackTransactionAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Dështoi rollback-u i transaksionit.");
        }
    }
}