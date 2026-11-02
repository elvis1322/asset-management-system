using Domain.Entities;
using Domain.IRepository;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
namespace Persistence.Repositories;

public class AssetsRepository : IAssetsRepository
{
    private readonly DataContext _context;
    public AssetsRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<(List<Asset> Items, int TotalCount)> GetPagedAssetsAsync(
      string? term,
      AssetStatus? status,
      AssetCategory? category,
      Guid? departmentId,
      int pageNumber,
      int pageSize,
      CancellationToken cancellationToken = default)
    {
        var query = _context.Assets
             .AsNoTracking()
             .AsSplitQuery()
             .Include(a => a.Department)
             .Include(a => a.Assignments.Where(aa => aa.IsActive))
                 .ThenInclude(aa => aa.Employee)
             .AsQueryable();
        if (!string.IsNullOrWhiteSpace(term))
        {
            term = term.Trim().ToLower();
            query = query.Where(a =>
                a.Name.ToLower().Contains(term) ||
                a.AssetTag.ToLower().Contains(term) ||
                a.SerialNumber.ToLower().Contains(term) ||
                (a.Department != null && a.Department.Name.ToLower().Contains(term))
            );
        }
        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }
        if (category.HasValue)
        {
            query = query.Where(a => a.Category == category.Value);
        }
        if (departmentId.HasValue)
        {
            query = query.Where(a => a.DepartmentId == departmentId.Value);
        }
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(a => a.Department)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<(List<Asset> Items, int TotalCount)> GetPagedByEmployeeIdAsync(
    Guid employeeId,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken = default)
    {
        var query = _context.Assets
           .AsNoTracking()
           .Where(a => a.Assignments.Any(aa =>
               aa.EmployeeId == employeeId && aa.IsActive));
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .ThenBy(a => a.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(a => a.Department)
            .Include(a => a.Assignments.Where(aa => aa.IsActive))
                .ThenInclude(aa => aa.Employee)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<Asset?> GetByIdNoTrackingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Assets
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Department)
            .Include(a => a.Assignments)
                .ThenInclude(aa => aa.Employee)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Asset?> GetByTrackingIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Assets
            .Include(a => a.Department)
            .Include(a => a.Assignments)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<List<Asset>> GetByIdsWithTrackingAsync(List<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Assets
            .Where(a => ids.Contains(a.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Asset?> GetByTagOrSerialAsync(string assetTag, string serialNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AssetTag == assetTag ||
                                   (!string.IsNullOrEmpty(serialNumber) && a.SerialNumber == serialNumber), cancellationToken);
    }

    public async Task<(List<Asset> Items, int TotalCount)> GetPagedByDepartmentIdAsync(
        Guid departmentId,
        AssetStatus? status,
        AssetCategory? category,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Assets
            .AsNoTracking()
            .AsSplitQuery()
            .Include(a => a.Department)
            .Include(a => a.Assignments.Where(aa => aa.IsActive))
                .ThenInclude(aa => aa.Employee)
            .Where(a => a.DepartmentId == departmentId)
            .AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }
        if (category.HasValue)
        {
            query = query.Where(a => a.Category == category.Value);
        }
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<IEnumerable<Asset>> GetAssetsByEmployeeIdAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Assets
            .AsNoTracking()
            .Include(a => a.Department)
          .Include(a => a.Assignments)
                .ThenInclude(aa => aa.Employee)
            .Where(a => a.Assignments.Any(aa => aa.EmployeeId == employeeId && aa.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<(List<AssetAssignment> Items, int TotalCount)> GetAssignmentHistoryPagedAsync(
    Guid assetId,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken = default)
    {
        var query = _context.AssetAssignments
            .AsNoTracking()
            .Where(aa => aa.AssetId == assetId);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(aa => aa.AssignedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(aa => aa.Employee)
            .ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<AssetAssignment?> GetActiveAssignmentAsync(
        Guid assetId,
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AssetAssignments
            .FirstOrDefaultAsync(aa => aa.AssetId == assetId && aa.EmployeeId == employeeId && aa.IsActive, cancellationToken);
    }

    public async Task AddAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        await _context.Assets.AddAsync(asset, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        _context.Assets.Update(asset);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Asset asset, CancellationToken cancellationToken = default)
    {
        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAssignmentAsync(AssetAssignment assignment, CancellationToken cancellationToken = default)
    {
        _context.AssetAssignments.Update(assignment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddAssignmentAsync(AssetAssignment assignment, CancellationToken cancellationToken = default)
    {
        await _context.AssetAssignments.AddAsync(assignment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> UpdateDepartmentAsync(List<Guid> assetIds, Guid newDepartmentId, CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        return await _context.Assets
            .Where(a => assetIds.Contains(a.Id))
            .ExecuteUpdateAsync(s => s
                .SetProperty(a => a.DepartmentId, newDepartmentId),
                cancellationToken);
    }

    public async Task<int> DeleteMultipleAsync(List<Guid> assetIds, CancellationToken cancellationToken = default)
    {
        return await _context.Assets
            .Where(a => assetIds.Contains(a.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetDepartmentCountsForAssetsAsync(List<Guid> assetIds, Guid newDepartmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Assets
            .Where(a => assetIds.Contains(a.Id) && a.DepartmentId != newDepartmentId)
            .GroupBy(a => a.DepartmentId)
            .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DepartmentId, x => x.Count, cancellationToken);
    }

    public async Task UnassignAssetsForEmployeesAsync(List<Guid> employeeIds, CancellationToken cancellationToken = default)
    {
        if (employeeIds == null || !employeeIds.Any()) return;
        var utcNow = DateTime.UtcNow;
        var activeAssignments = await _context.AssetAssignments
            .Where(aa => employeeIds.Contains(aa.EmployeeId) && aa.IsActive)
            .ToListAsync(cancellationToken);
        if (!activeAssignments.Any()) return;
        foreach (var assignment in activeAssignments)
        {
            assignment.IsActive = false;
            assignment.ReturnedDate = utcNow;
        }
        var assetIds = activeAssignments.Select(aa => aa.AssetId).Distinct().ToList();
        var assets = await _context.Assets
                  .Include(a => a.Assignments)
                  .Where(a => assetIds.Contains(a.Id))
                  .ToListAsync(cancellationToken);
        foreach (var asset in assets)
        {
            bool hasOtherActive = asset.Assignments != null &&
                      asset.Assignments.Any(a => a.IsActive && !employeeIds.Contains(a.EmployeeId));
            if (!hasOtherActive)
            {
                asset.Status = AssetStatus.InStock;
                _context.Assets.Update(asset);
            }
        }
        await _context.SaveChangesAsync(cancellationToken);
    }

public async Task<List<Asset>> GetAssetsExpiringInDaysAsync(int days, CancellationToken cancellationToken = default)
{
    var targetDate = DateTime.UtcNow.Date.AddDays(days);
    return await _context.Assets
        .Where(a => a.WarrantyExpirationDate.HasValue && a.WarrantyExpirationDate.Value.Date == targetDate)
        .ToListAsync(cancellationToken);
}

public async Task<List<Asset>> GetExpiredAssetsAsync(CancellationToken cancellationToken = default)
{
    var today = DateTime.UtcNow.Date;
    return await _context.Assets
        .Where(a => a.WarrantyExpirationDate.HasValue && a.WarrantyExpirationDate.Value.Date < today)
        .ToListAsync(cancellationToken);
}
}