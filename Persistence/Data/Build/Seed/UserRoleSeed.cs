using Domain.Entities;
using Domain.Entities.Base;
using Microsoft.AspNetCore.Identity;
namespace Persistence.Data.Build.Seed;

public static class UserRoleSeeder
{

    public static async Task SeedAsync(UserManager<User> userManager)
    {
        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
        var adminFirstName = Environment.GetEnvironmentVariable("ADMIN_FIRST_NAME") ?? "Admin";
        var adminLastName = Environment.GetEnvironmentVariable("ADMIN_LAST_NAME") ?? "User";
        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException("ADMIN_EMAIL and ADMIN_PASSWORD environment variables must be set.");
        }
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = adminFirstName,
                LastName = adminLastName,
                UserName = adminEmail,
                Gender = "M",
                Email = adminEmail,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create Admin user: {errors}");
            }
        }
        if (!await userManager.IsInRoleAsync(adminUser, AppDefaults.AppRoles.Admin))
        {
            await userManager.AddToRoleAsync(adminUser, AppDefaults.AppRoles.Admin);
        }
    }
}