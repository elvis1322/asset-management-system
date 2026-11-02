using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Domain.Entities.Base;
namespace Persistence.Data;

public class DataContext : IdentityDbContext<
    User,
    Role,
    Guid,
    IdentityUserClaim<Guid>,
    UserRole,
    IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>,
    IdentityUserToken<Guid>>
{
    private readonly IHttpContextAccessor? _httpContextAccessor;
    public DataContext(
        DbContextOptions<DataContext> options,
        IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<Employee> Employees { get; set; } = null!;
    public DbSet<Asset> Assets { get; set; } = null!;
    public DbSet<Department> Departments { get; set; } = null!;
    public DbSet<AssetAssignment> AssetAssignments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
    }
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentUserIdString = _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        Guid? currentUserId = Guid.TryParse(currentUserIdString, out var parsedId) ? parsedId : null;

        var utcNow = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    if (currentUserId.HasValue) entry.Entity.CreatedBy = currentUserId.ToString();
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedAt = utcNow;
                    if (currentUserId.HasValue) entry.Entity.LastModifiedBy = currentUserId.ToString();
                    entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(IAuditableEntity.CreatedBy)).IsModified = false;
                    break;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}