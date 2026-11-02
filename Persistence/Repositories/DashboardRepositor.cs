using Domain.Entities.Models.Dashboard;
using Domain.IRepository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Persistence.Data;
namespace Persistence.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly DataContext _context;
    public DashboardRepository(DataContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetTotalPurchaseValueAsync(Guid? departmentId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Assets
            .AsNoTracking()
            .Where(a => !departmentId.HasValue || a.DepartmentId == departmentId.Value)
            .SumAsync(a => a.PurchaseValue, cancellationToken);
    }

 public async Task<List<DepartmentValueModel>> GetValueByDepartmentAsync(
    Guid? departmentId = null,
    CancellationToken cancellationToken = default)
{
    var deptParam = new SqlParameter(
        "@DepartmentId",
        (object?)departmentId ?? DBNull.Value);

    var query = _context.Database.SqlQueryRaw<DepartmentValueModel>(
        "EXEC dbo.sp_GetDepartmentValueAnalytics @DepartmentId",
        deptParam);

    return await query.ToListAsync(cancellationToken);
}

    public async Task<List<StatusBreakdownModel>> GetStatusBreakdownAsync(Guid? departmentId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Assets
            .AsNoTracking()
            .Where(a => !departmentId.HasValue || a.DepartmentId == departmentId.Value)
            .GroupBy(a => a.Status)
            .Select(g => new StatusBreakdownModel
            {
                Status = g.Key.ToString(),
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CategoryCountModel>> GetAssetsByCategoryAsync(Guid? departmentId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Assets
            .AsNoTracking()
            .Where(a => !departmentId.HasValue || a.DepartmentId == departmentId.Value)
            .GroupBy(a => a.Category)
            .Select(g => new CategoryCountModel
            {
                Category = g.Key.ToString(),
                Count = g.Count()
            })
            .OrderByDescending(c => c.Count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AgeDistributionModel>> GetAgeDistributionAsync(Guid? departmentId = null, CancellationToken cancellationToken = default)
    {
        var deptParam = new SqlParameter("@DepartmentId", (object?)departmentId ?? DBNull.Value);
        var rawList = await _context.Database
            .SqlQueryRaw<AgeDistributionRawResult>("EXEC dbo.sp_GetAssetAgeDistribution @DepartmentId", deptParam)
            .ToListAsync(cancellationToken);
        var result = rawList.FirstOrDefault();
        return new List<AgeDistributionModel>
        {
            new() { AgeRange = "< 1 Vit", Count = result?.UnderOne ?? 0 },
            new() { AgeRange = "1 - 3 Vjet", Count = result?.OneToThree ?? 0 },
            new() { AgeRange = "3 - 5 Vjet", Count = result?.ThreeToFive ?? 0 },
            new() { AgeRange = "5+ Vjet", Count = result?.OverFive ?? 0 }
        };
    }
}