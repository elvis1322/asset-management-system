using Domain.Entities;
using ErrorOr;
namespace Application.Interfaces;

public interface IUserAccountService
{
  Task<(List<User> Items, int TotalCount)> GetPagedUsersAsync(string? term, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
  Task<ErrorOr<User>> VerifyCredentialsAsync(string email, string password);
  Task<List<string>> GetUserRolesListAsync(Guid userId);
  Task<bool> ExistsByEmailAsync(string email);
  Task<ErrorOr<User>> CreateUserAsync(string email, string password, string firstName, string lastName, string gender, string role);
  Task<ErrorOr<Success>> UpdateUserIdentityAsync(Guid userId, string firstName, string lastName, string email, string gender);
  Task<ErrorOr<Success>> DeleteUserAsync(Guid userId);
  Task<User?> GetUserByIdAsync(Guid userId);
  Task<User?> GetUserByEmailAsync(string email);
  Task<string> GetUserRoleAsync(Guid userId);
  Task<ErrorOr<Success>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
  Task<string?> GeneratePasswordResetTokenAsync(string email);
  Task<ErrorOr<Success>> ResetPasswordAsync(string email, string token, string newPassword);
  Task<ErrorOr<string>> GenerateEmailChangeTokenAsync(Guid userId, string newEmail);
  Task<ErrorOr<Success>> ConfirmEmailChangeAsync(Guid userId, string newEmail, string token);
  Task<ErrorOr<Success>> AddToRoleAsync(Guid userId, string role);
  Task<ErrorOr<Success>> UpdateUserRolesAsync(Guid userId, List<string> newRoles);
}