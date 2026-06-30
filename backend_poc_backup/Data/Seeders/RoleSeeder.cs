using Microsoft.AspNetCore.Identity;
using MindLens.Api.Models.Enums;

namespace MindLens.Api.Data.Seeders;

public class RoleSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        
        foreach(var role in Enum.GetValues<UserRole>()) 
        {
            var roleName = role.ToString();

            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync((new IdentityRole<Guid>
                {
                    Name = roleName
                }));
            }
        }
    }
}