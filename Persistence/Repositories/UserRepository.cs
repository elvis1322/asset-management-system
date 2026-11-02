using Domain.Entities;
using Domain.IRepository;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
namespace Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;
    public UserRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<(List<User> Items, int TotalCount)> GetPagedUsersAsync(
        string? term,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(term))
        {
            var cleanTerm = term.Trim();
            query = query.Where(u =>
                (u.UserName != null && u.UserName.Contains(cleanTerm)) ||
                (u.Email != null && u.Email.Contains(cleanTerm))
            );
        }
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(u => u.UserName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)   
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }
}