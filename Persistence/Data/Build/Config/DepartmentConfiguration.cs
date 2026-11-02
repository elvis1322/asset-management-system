using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Persistence.Data.Build.Config;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{

    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);
        builder.HasIndex(d => d.Name)
            .IsUnique();
        builder.Property(d => d.Code)
            .HasMaxLength(20);
        builder.HasIndex(d => d.Code)
            .IsUnique()
            .HasFilter("[Code] IS NOT NULL");
        builder.Property(d => d.Description)
            .HasMaxLength(500);
               builder.ToTable(table => table.UseSqlOutputClause(false));
    }
}