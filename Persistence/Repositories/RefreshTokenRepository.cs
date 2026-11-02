using Domain.Entities;
using Domain.IRepository;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
namespace Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly DataContext _dataContext;
    public RefreshTokenRepository(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _dataContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _dataContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dataContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserToken == token, cancellationToken);
    }

    public async Task<List<RefreshToken>> GetAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dataContext.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _dataContext.RefreshTokens.Update(refreshToken);
        await _dataContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(string token, CancellationToken cancellationToken = default)
    {
        var deletedCount = await _dataContext.RefreshTokens
            .Where(rt => rt.UserToken == token)
            .ExecuteDeleteAsync(cancellationToken);
        return deletedCount > 0;
    }

    public async Task<bool> DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var deletedCount = await _dataContext.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
        return deletedCount > 0;
    }
}