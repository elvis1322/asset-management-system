using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Persistence.Data.Build.Config;

public class AssetAssignmentConfiguration : IEntityTypeConfiguration<AssetAssignment>
{

    public void Configure(EntityTypeBuilder<AssetAssignment> builder)
    {
        builder.HasKey(aa => aa.Id);
        builder.HasIndex(aa => aa.EmployeeId);
        builder.HasIndex(aa => new { aa.AssetId, aa.EmployeeId })
                  .HasFilter("[IsActive] = 1");
        builder.Property(aa => aa.Notes)
            .HasMaxLength(500);
        builder.HasOne(aa => aa.Asset)
            .WithMany(a => a.Assignments)
            .HasForeignKey(aa => aa.AssetId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(aa => aa.Employee)
           .WithMany(e => e.Assignments)
           .HasForeignKey(aa => aa.EmployeeId)
           .OnDelete(DeleteBehavior.Cascade); 
              builder.ToTable(table => table.UseSqlOutputClause(false));
    }
}