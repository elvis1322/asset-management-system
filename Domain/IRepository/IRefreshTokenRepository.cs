using Domain.Entities;
namespace Domain.IRepository;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<List<RefreshToken>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}