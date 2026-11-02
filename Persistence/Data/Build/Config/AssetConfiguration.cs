using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Persistence.Data.Build.Config;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{

    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.AssetTag)
            .IsRequired()
            .HasMaxLength(50);
        builder.HasIndex(a => a.AssetTag)
            .IsUnique();
        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.Property(a => a.Category)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();
        builder.Property(a => a.SerialNumber)
            .HasMaxLength(100);
        builder.HasIndex(a => a.SerialNumber)
            .HasFilter("[SerialNumber] IS NOT NULL");
        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
        builder.HasIndex(a => a.Status);
        builder.Property(a => a.PurchaseValue)
            .HasPrecision(18, 2);
        builder.Property(a => a.IsShared)
            .HasDefaultValue(false);
        builder.HasIndex(a => a.DepartmentId);
        builder.HasOne(a => a.Department)
            .WithMany(d => d.Assets)
            .HasForeignKey(a => a.DepartmentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        builder.Property(a => a.Status)
         .HasConversion<string>();
         builder.ToTable(table => table.UseSqlOutputClause(false));
    }
}