using Microsoft.AspNetCore.Identity;
using Domain.Entities;
using ErrorOr;
using Application.Interfaces;
using Domain.IRepository;
using Application.Models;
namespace Application.Services;

public class UserAccountService : IUserAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<User> _userManager;
    public UserAccountService(UserManager<User> userManager, IUserRepository userRepository)
    {
        _userManager = userManager;
        _userRepository = userRepository;
    }

    public Task<(List<User> Items, int TotalCount)> GetPagedUsersAsync(
      string? term,
      int pageNumber,
      int pageSize,
      CancellationToken cancellationToken = default)
    {
        return _userRepository.GetPagedUsersAsync(
            term,
            pageNumber,
            pageSize,
            cancellationToken);
    }

    public async Task<ErrorOr<User>> VerifyCredentialsAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Error.Unauthorized("Auth.InvalidCredentials", "Kredencialet janë të pasakta.");
        }
        if (await _userManager.IsLockedOutAsync(user))
        {
            var lockoutEndDate = await _userManager.GetLockoutEndDateAsync(user);
            var remainingMinutes = lockoutEndDate.HasValue
                ? Math.Ceiling((lockoutEndDate.Value - DateTimeOffset.UtcNow).TotalMinutes)
                : 5;
            return Error.Custom(
                type: 429,
                code: "Auth.AccountLocked",
                description: $"Llogaria juaj është e bllokuar për shkak të tentativave të shumta. Provojeni përsëri pas {remainingMinutes} minutash."
            );
        }
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
        {
            await _userManager.AccessFailedAsync(user);
            return Error.Unauthorized("Auth.InvalidCredentials", "Kredencialet janë të pasakta.");
        }
        await _userManager.ResetAccessFailedCountAsync(user);
        return user;
    }

    public async Task<List<string>> GetUserRolesListAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return new List<string>();
        var roles = await _userManager.GetRolesAsync(user);
        return roles.ToList();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        if (userId == Guid.Empty) return null;
        return await _userManager.FindByIdAsync(userId.ToString());
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email) is not null;
    }

    public async Task<ErrorOr<User>> CreateUserAsync(string email, string password, string firstName, string lastName, string gender, string role)
    {
        var normalizedGender = GenderHelper.NormalizeGender(gender);
        if (normalizedGender is null)
        {
            return Error.Validation("User.InvalidGender", "Gjinia e dhënë nuk është e vlefshme. Përdorni Male ose Female.");
        }
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Gender = normalizedGender,
            IsActive = true,
            EmailConfirmed = true
        };
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return Error.Validation("User.CreationFailed", result.Errors.First().Description);
        }
        if (!string.IsNullOrWhiteSpace(role))
        {
            var roleResult = await _userManager.AddToRoleAsync(user, role.Trim());
            if (!roleResult.Succeeded)
            {
                return Error.Validation("User.RoleAssignmentFailed", "Caktimi i rolit dështoi.");
            }
        }
        return user;
    }

    public async Task<ErrorOr<Success>> UpdateUserIdentityAsync(Guid userId, string firstName, string lastName, string email, string gender)
    {
        if (userId == Guid.Empty) return Error.Validation("User.InvalidId", "ID e përdoruesit nuk është e vlefshme.");
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Error.NotFound("User.NotFound", "Përdoruesi nuk u gjet.");
        var normalizedGender = GenderHelper.NormalizeGender(gender);
        if (normalizedGender is null)
        {
            return Error.Validation("User.InvalidGender", "Gjinia e dhënë nuk është e vlefshme. Përdorni Male ose Female.");
        }
        user.FirstName = firstName;
        user.LastName = lastName;
        user.Email = email;
        user.UserName = email;
        user.Gender = normalizedGender;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errorDescription = updateResult.Errors.First().Description;
            return Error.Validation("User.UpdateFailed", errorDescription);
        }
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> DeleteUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Error.NotFound("User.NotFound", "Përdoruesi nuk u gjet.");
        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return Error.Failure("User.DeleteFailed", "Fshirja e përdoruesit dështoi.");
        }
        return Result.Success;
    }

    public async Task<string> GetUserRoleAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return string.Empty;
        var roles = await _userManager.GetRolesAsync(user);
        return roles.FirstOrDefault() ?? string.Empty;
    }

    public async Task<ErrorOr<Success>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Error.NotFound("Auth.UserNotFound", "Përdoruesi nuk u gjet.");
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return Error.Validation("Auth.ChangePasswordFailed", errors);
        }
        await _userManager.UpdateSecurityStampAsync(user);
        return Result.Success;
    }

    public async Task<string?> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return null;
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<ErrorOr<Success>> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return Error.Validation("Auth.ResetPasswordFailed", "Token-i është i pavlefshëm ose ka skaduar.");
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            var firstError = result.Errors.FirstOrDefault()?.Description ?? "Token-i është i pavlefshëm ose ka skaduar.";
            return Error.Validation("Auth.ResetPasswordFailed", firstError);
        }
        return Result.Success;
    }

    public async Task<ErrorOr<string>> GenerateEmailChangeTokenAsync(Guid userId, string newEmail)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Error.NotFound("User.NotFound", "Përdoruesi nuk u gjet.");
        if (await _userManager.FindByEmailAsync(newEmail) is not null)
        {
            return Error.Conflict("User.EmailAlreadyExists", "Ky email është tashmë në përdorim.");
        }
        return await _userManager.GenerateChangeEmailTokenAsync(user, newEmail);
    }

    public async Task<ErrorOr<Success>> ConfirmEmailChangeAsync(Guid userId, string newEmail, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Error.NotFound("User.NotFound", "Përdoruesi nuk u gjet.");
        var result = await _userManager.ChangeEmailAsync(user, newEmail, token);
        if (!result.Succeeded)
        {
            var firstError = result.Errors.FirstOrDefault()?.Description ?? "Ndryshimi i email-it dështoi.";
            return Error.Validation("Auth.ChangeEmailFailed", firstError);
        }
        await _userManager.SetUserNameAsync(user, newEmail);
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> AddToRoleAsync(Guid userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Error.NotFound("User.NotFound", "Përdoruesi nuk u gjet.");
        var result = await _userManager.AddToRoleAsync(user, role.Trim());
        if (!result.Succeeded)
        {
            return Error.Validation("User.RoleAssignmentFailed", "Caktimi i rolit dështoi.");
        }
        return Result.Success;
    }

    public async Task<ErrorOr<Success>> UpdateUserRolesAsync(Guid userId, List<string> newRoles)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return Error.NotFound("User.NotFound", "Përdoruesi nuk u gjet.");
        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            return Error.Validation("User.RoleRemoveFailed", "Heqja e roleve ekzistuese dështoi.");
        }
        if (newRoles.Any())
        {
            var addResult = await _userManager.AddToRolesAsync(user, newRoles.Select(r => r.Trim()));
            if (!addResult.Succeeded)
            {
                return Error.Validation("User.RoleAssignmentFailed", "Caktimi i roleve të reja dështoi.");
            }
        }
        return Result.Success;
    }
}