using ErrorOr;
using Application.DTO.Auth;
namespace Application.Interfaces;

public interface IAuthService
{
    Task<ErrorOr<UserResponseDto>> RegisterAsync(RegisterUserDto dto, CancellationToken cancellationToken = default);
    Task<ErrorOr<UserResponseLoginDto>> LoginAsync(UserLoginDto dto, CancellationToken cancellationToken = default);
    Task<ErrorOr<bool>> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<ErrorOr<UserResponseLoginDto>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}