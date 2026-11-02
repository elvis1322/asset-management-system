using Domain.Entities;
using Domain.Entities.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace Persistence.Data.Build.Seed;

public class RoleSeed
{

    public static async Task SeedAsync(RoleManager<Role> roleManager)
    {
        var appRoles = AppDefaults.AppRoles.AllRoles;
        foreach (var roleName in appRoles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    NormalizedName = roleName.ToUpperInvariant()
                };
                await roleManager.CreateAsync(role);
            }
        }
        var dbRoles = await roleManager.Roles.ToListAsync();
        foreach (var dbRole in dbRoles)
        {
            if (!appRoles.Contains(dbRole.Name!, StringComparer.OrdinalIgnoreCase))
            {
                await roleManager.DeleteAsync(dbRole);
            }
        }
    }    
}