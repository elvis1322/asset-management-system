using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Persistence.Data.Build.Config;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{

    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => e.Email).IsUnique();
        builder.Property(e => e.Position).HasMaxLength(100);
        builder.HasIndex(e => e.UserId)
            .IsUnique()
            .HasFilter("[UserId] IS NOT NULL"); 
        builder.HasIndex(e => e.DepartmentId);
        builder.HasOne(e => e.User)
            .WithOne(u => u.Employee)
            .HasForeignKey<Employee>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
               builder.ToTable(table => table.UseSqlOutputClause(false));
    }  
}