using Domain.Entities;
using Domain.IRepository;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
namespace Persistence.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly DataContext _context;
    public EmployeeRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<(List<Employee> Items, int TotalCount)> GetPagedEmployeesAsync(string? term, Guid? departmentId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Assignments.Where(a => a.IsActive))
            .AsQueryable();
        if (departmentId.HasValue)
        {
            query = query.Where(e => e.DepartmentId == departmentId.Value);
        }
        if (!string.IsNullOrWhiteSpace(term))
        {
            var cleanTerm = term.Trim();
            bool isGuid = Guid.TryParse(cleanTerm, out Guid parsedGuid);
            query = query.Where(e =>
                (e.FirstName != null && e.FirstName.Contains(cleanTerm)) ||
                (e.LastName != null && e.LastName.Contains(cleanTerm)) ||
                (e.Email != null && e.Email.Contains(cleanTerm)) ||
                (e.Position != null && e.Position.Contains(cleanTerm)) ||
                (isGuid && e.Id == parsedGuid)
            );
        }
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(e => e.LastName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<Employee?> GetByIdNoTrackingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .AsNoTracking()
            .AsSplitQuery()
            .Include(e => e.Department)
            .Include(e => e.Assignments)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Employee?> GetByIdTrackingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .AsSplitQuery()
            .Include(e => e.Department)
            .Include(e => e.Assignments)
                .ThenInclude(aa => aa.Asset)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Employee?> GetByUserIdNoTrackingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);
    }

    public async Task<Employee?> GetByUserIdTrackingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .AsSplitQuery()
            .Include(e => e.Department)
            .Include(e => e.Assignments)
                .ThenInclude(aa => aa.Asset)
            .FirstOrDefaultAsync(e => e.UserId == userId, cancellationToken);
    }

   public async Task<(List<Employee> Items, int TotalCount)> GetPagedByDepartmentIdAsync(
    Guid departmentId,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken = default)
{
    var query = _context.Employees
        .AsNoTracking()
        .Where(e => e.DepartmentId == departmentId);
    int totalCount = await query.CountAsync(cancellationToken);
    var items = await query
        .Include(e => e.Department)
        .OrderBy(e => e.LastName) 
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);
    return (items, totalCount);
}

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        await _context.Employees.AddAsync(employee, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<Employee>> GetByIdsWithTrackingAsync(List<Guid> employeeIds, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Where(e => employeeIds.Contains(e.Id))
            .AsSplitQuery()
            .Include(e => e.Department)
            .Include(e => e.Assignments)
                .ThenInclude(aa => aa.Asset)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> DeleteMultipleAsync(List<Guid> employeeIds, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Where(e => employeeIds.Contains(e.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetDepartmentCountsForDeleteAsync(List<Guid> employeeIds, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Where(e => employeeIds.Contains(e.Id) && e.DepartmentId != Guid.Empty)
            .GroupBy(e => e.DepartmentId)
            .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DepartmentId, x => x.Count, cancellationToken);
    }
}