using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MindLens.Api.Data;
using MindLens.Api.Models;
using MindLens.Api.Models.Enums;
using MindLens.Api.Services.Interfaces;

namespace MindLens.Api.Data.Seeders;

public static class RawSqlSeeder
{
    public static async Task SeedTenantsAsync(
        IServiceProvider serviceProvider, 
        ApplicationContext masterContext,
        UserManager<User> userManager)
    {
        var tenantConfigs = new[]
        {
            new { Email = "carlos@mindlens.com", Prefix = "02-carlos", VectorPrefix = "07-vectors_carlos" },
            new { Email = "ana@mindlens.com", Prefix = "03-ana", VectorPrefix = "07-vectors_ana" },
            new { Email = "luis@mindlens.com", Prefix = "04-luis", VectorPrefix = "07-vectors_luis" },
            new { Email = "marta@mindlens.com", Prefix = "05-marta", VectorPrefix = "07-vectors_marta" },
            new { Email = "pedro@mindlens.com", Prefix = "06-pedro", VectorPrefix = "07-vectors_pedro" }
        };

        var patients = new[]
        {
            new { GlobalId = Guid.Parse("b22407c4-d863-5688-93b8-bf0521624c08"), FirstName = "Juan", LastName = "Perez", Email = "juan.perez@email.com", PsyEmail = "carlos@mindlens.com" },
            new { GlobalId = Guid.Parse("0870d4ca-aac8-507a-ab61-7cc7ca4c62d6"), FirstName = "Ana", LastName = "Lopez", Email = "ana.lopez@email.com", PsyEmail = "carlos@mindlens.com" },
            new { GlobalId = Guid.Parse("64c8cd2c-c8cf-5f1c-82d3-8051c3771c26"), FirstName = "Luis", LastName = "Martinez", Email = "luis.m@email.com", PsyEmail = "ana@mindlens.com" },
            new { GlobalId = Guid.Parse("2f94e082-3eac-597d-93c3-47aa8c540748"), FirstName = "Sofia", LastName = "Castro", Email = "sofia.castro@email.com", PsyEmail = "ana@mindlens.com" },
            new { GlobalId = Guid.Parse("4d75d05a-acae-5937-8992-601ec7016ede"), FirstName = "Roberto", LastName = "Diaz", Email = "roberto.diaz@email.com", PsyEmail = "luis@mindlens.com" },
            new { GlobalId = Guid.Parse("041c8d34-263c-585b-9a8e-63784c38c3de"), FirstName = "Diana", LastName = "Ruiz", Email = "diana.ruiz@email.com", PsyEmail = "luis@mindlens.com" },
            new { GlobalId = Guid.Parse("6a5ae6eb-71b7-5b62-949f-cff2c7f80df6"), FirstName = "Carlos", LastName = "Vargas", Email = "carlos.vargas@email.com", PsyEmail = "luis@mindlens.com" },
            new { GlobalId = Guid.Parse("630139d5-04d8-5e97-8d76-a46e0cc9f0db"), FirstName = "Maria", LastName = "Gomez", Email = "maria.gomez@email.com", PsyEmail = "marta@mindlens.com" },
            new { GlobalId = Guid.Parse("8e41cfe4-3250-5a3a-90fa-ecb1c5becb2d"), FirstName = "Pedro", LastName = "Alonso", Email = "pedro.alonso@email.com", PsyEmail = "marta@mindlens.com" },
            new { GlobalId = Guid.Parse("24f85eb0-6ac7-5645-a531-a2e80bbbfae5"), FirstName = "Lucas", LastName = "Torres", Email = "lucas.torres@email.com", PsyEmail = "pedro@mindlens.com" },
            new { GlobalId = Guid.Parse("fd8489cb-8a7f-58d8-a53c-20ca7610b7a0"), FirstName = "Valeria", LastName = "Rios", Email = "valeria.rios@email.com", PsyEmail = "pedro@mindlens.com" },
            new { GlobalId = Guid.Parse("ed829d02-ee9b-510f-8cc0-cab2ffdcc0c3"), FirstName = "Mateo", LastName = "Blanco", Email = "mateo.blanco@email.com", PsyEmail = "pedro@mindlens.com" }
        };

        // 1. Create Patients in Master DB
        foreach (var p in patients)
        {
            var user = await userManager.FindByIdAsync(p.GlobalId.ToString());
            if (user == null)
            {
                user = new User
                {
                    Id = p.GlobalId,
                    UserName = p.Email,
                    Email = p.Email,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    Role = UserRole.Patient
                };
                await userManager.CreateAsync(user, "Test1234!");
            }

            // Find Psy
            var psy = await userManager.FindByEmailAsync(p.PsyEmail);
            if (psy != null)
            {
                // Create Registry
                var exists = await masterContext.TreatmentsRegistry.AnyAsync(tr => tr.PatientId == user.Id && tr.PsychologisstId == psy.Id);
                if (!exists)
                {
                    masterContext.TreatmentsRegistry.Add(new TreatmentRegistry
                    {
                        Id = Guid.NewGuid(),
                        PatientId = user.Id,
                        PsychologisstId = psy.Id,
                        State = TreatmentRegistryState.InProcess
                    });
                }
            }
        }
        await masterContext.SaveChangesAsync();

        // 2. Execute Tenant SQL Seeds
        var config = serviceProvider.GetRequiredService<IConfiguration>();
        var tenantService = serviceProvider.GetRequiredService<ITenantService>();
        
        var baseDir = Path.Combine(AppContext.BaseDirectory, "database", "seeds");
        if (!Directory.Exists(baseDir))
        {
            Console.WriteLine($"[RawSqlSeeder] Seeds directory not found at {baseDir}");
            return;
        }

        foreach (var tc in tenantConfigs)
        {
            var psy = await userManager.FindByEmailAsync(tc.Email);
            if (psy == null) continue;

            var tenant = await masterContext.Tenants.FirstOrDefaultAsync(t => t.PsychologistId == psy.Id);
            if (tenant == null) continue;

            Console.WriteLine($"[RawSqlSeeder] Seeding tenant {tenant.DatabaseName}...");

            var optionsBuilder = new DbContextOptionsBuilder<TenantContext>();
            var connectionString = config.GetConnectionString("SharedDb");
            var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString) { Database = tenant.DatabaseName };
            optionsBuilder.UseNpgsql(builder.ConnectionString, o => o.UseVector());

            using var tenantDb = new TenantContext(optionsBuilder.Options);

            var sqlFile = Path.Combine(baseDir, $"{tc.Prefix}_seed.sql");
            if (File.Exists(sqlFile))
            {
                // Check if already seeded by checking if any patient exists
                if (!await tenantDb.Patients.AnyAsync())
                {
                    Console.WriteLine($"[RawSqlSeeder] Executing {sqlFile}");
                    var sql = await File.ReadAllTextAsync(sqlFile);
                    await tenantDb.Database.ExecuteSqlRawAsync(sql);
                }
            }

            var vectorFile = Path.Combine(baseDir, $"{tc.VectorPrefix}.sql");
            if (File.Exists(vectorFile))
            {
                // Simple check if vectors are seeded (check if JournalingRegisters has rows)
                if (!await tenantDb.JournalingRegisters.AnyAsync())
                {
                    Console.WriteLine($"[RawSqlSeeder] Executing {vectorFile}");
                    var sql = await File.ReadAllTextAsync(vectorFile);
                    tenantDb.Database.SetCommandTimeout(300); // 5 mins timeout for heavy inserts
                    await tenantDb.Database.ExecuteSqlRawAsync(sql);
                }
            }
        }
    }
}
