using ErrorOr;
using Application.DTO.Profile;
namespace Application.Interfaces;

public interface IProfileService
{
    Task<ErrorOr<ProfileResponseDto>> GetProfileAsync(CancellationToken cancellationToken = default);
    Task<ErrorOr<ProfileResponseDto>> UpdateProfileAsync(UpdateProfileDto dto, CancellationToken cancellationToken = default);
    Task<ErrorOr<bool>> ChangePasswordAsync(ChangePasswordDto dto, CancellationToken cancellationToken = default);
    Task<ErrorOr<bool>> ForgotPasswordAsync(ForgotPasswordDto dto, CancellationToken cancellationToken = default);
    Task<ErrorOr<bool>> ResetPasswordAsync(ResetPasswordDto dto, CancellationToken cancellationToken = default);
    Task<ErrorOr<bool>> RequestEmailChangeAsync(RequestEmailChangeDto dto, CancellationToken cancellationToken = default);
    Task<ErrorOr<bool>> ConfirmEmailChangeAsync(ConfirmEmailChangeDto dto, CancellationToken cancellationToken = default);
}