using Domain.Entities;
using Domain.IRepository;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
namespace Persistence.Repositories;

public class DepartmentsRepository : IDepartmentsRepository
{
    private readonly DataContext _context;
    public DepartmentsRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<Department?> GetByIdNoTrackingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Department?> GetByIdTrackingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<bool> IsEmployeeInDepartmentAsync(Guid departmentId, Guid employeeId, CancellationToken cancellationToken)
    {
        return await _context.Employees
            .AnyAsync(e => e.Id == employeeId && e.DepartmentId == departmentId, cancellationToken);
    }

    public async Task<bool> IsAssetInDepartmentAsync(Guid departmentId, Guid assetId, CancellationToken cancellationToken)
    {
        return await _context.Assets
            .AnyAsync(a => a.Id == assetId && a.DepartmentId == departmentId, cancellationToken);
    }

    public async Task<IEnumerable<Department>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Departments
         .ToListAsync(cancellationToken);
    }

    public async Task<(List<Department> Items, int TotalCount)> GetPagedDepartmentsAsync(
        string? term,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Departments
            .AsNoTracking()
            .AsSplitQuery();
        if (!string.IsNullOrWhiteSpace(term))
        {
            query = query.Where(d => d.Name.ToLower().Contains(term.ToLower()) ||
                                     d.Code.ToLower().Contains(term.ToLower()));
        }
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(d => d.Name)
            .ThenBy(d => d.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new Department
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                EmployeeCount = d.Employees.Count(e => e.IsActive),
                AssetCount = d.Assets.Count()
            })
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<Department?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => EF.Functions.Like(d.Name, name.Trim()), cancellationToken);
    }

    public async Task AddAsync(Department department, CancellationToken cancellationToken = default)
    {
        await _context.Departments.AddAsync(department, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Department department, CancellationToken cancellationToken = default)
    {
        _context.Departments.Update(department);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Department department, CancellationToken cancellationToken = default)
    {
        _context.Departments.Remove(department);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Department?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(d => d.Code == code, cancellationToken);
    }
}