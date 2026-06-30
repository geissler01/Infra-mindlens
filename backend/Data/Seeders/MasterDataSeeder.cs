using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MindLens.Api.Models;
using MindLens.Api.Models.Enums;

namespace MindLens.Api.Data.Seeders;

public class MasterDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var context = serviceProvider.GetRequiredService<ApplicationContext>();

        var tenantsToSeed = new List<(string DatabaseName, string Email, string FirstName, string LastName)>
        {
            ("db_mindlens_carlos", "carlos@mindlens.com", "Carlos", "Ramirez"),
            ("db_mindlens_ana", "ana@mindlens.com", "Ana", "Gomez"),
            ("db_mindlens_luis", "luis@mindlens.com", "Luis", "Fernandez"),
            ("db_mindlens_marta", "marta@mindlens.com", "Marta", "Lopez"),
            ("db_mindlens_pedro", "pedro@mindlens.com", "Pedro", "Martinez")
        };

        foreach (var tenantData in tenantsToSeed)
        {
            // Verificamos si el usuario ya existe
            var existingUser = await userManager.FindByEmailAsync(tenantData.Email);
            if (existingUser == null)
            {
                var newUser = new User
                {
                    UserName = tenantData.Email,
                    Email = tenantData.Email,
                    FirstName = tenantData.FirstName,
                    LastName = tenantData.LastName,
                    Role = UserRole.Psychologist,
                    CreatedAt = DateTime.UtcNow
                };

                // Creamos usuario con contraseña genérica
                var result = await userManager.CreateAsync(newUser, "Password123!");
                if (result.Succeeded)
                {
                    // Asignar rol
                    await userManager.AddToRoleAsync(newUser, UserRole.Psychologist.ToString());

                    // Crear el Perfil (PsychologistProfile)
                    var profile = new PsychologistProfile
                    {
                        PsychologistId = newUser.Id,
                        Speciality = "General",
                        ExperienceYears = 5,
                        Biography = "Psicólogo de prueba generado por el seeder.",
                        ProfilePhotoUrl = ""
                    };
                    context.Profiles.Add(profile);

                    // Crear el Tenant apuntando a la base de datos correspondiente
                    var tenant = new Tenant
                    {
                        PsychologistId = newUser.Id,
                        DatabaseName = tenantData.DatabaseName,
                        State = TenantState.Available,
                        Domain = tenantData.DatabaseName.Replace("_db", ".mindlens.local")
                    };
                    context.Tenants.Add(tenant);
                    
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
