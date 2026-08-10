using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

public static class RoleHelper
{
    public static async Task EnsureRolesCreated(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Admin", "Client", "Owner" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}