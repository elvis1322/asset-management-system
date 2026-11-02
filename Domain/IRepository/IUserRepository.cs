using Domain.Entities;
namespace Domain.IRepository;

public interface IUserRepository
{
    Task<(List<User> Items, int TotalCount)> GetPagedUsersAsync(string? term, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}