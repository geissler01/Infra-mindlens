using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using MindLens.Api.Data;
using MindLens.Api.Models;
using MindLens.Api.Services;
using MindLens.Api.Services.Interfaces;
using MindLens.Api.Data.Seeders;
using Microsoft.AspNetCore.Identity;

Console.WriteLine("==================================================");
Console.WriteLine("STARTING MINDLENS MIGRATOR SERVICE");
Console.WriteLine("==================================================");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) => {
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) => {
        services.AddScoped<ITenantService, TenantService>();
        
        services.AddDbContext<ApplicationContext>(options =>
        {
            options.UseNpgsql(context.Configuration.GetConnectionString("SharedDb"));
        });

        services.AddDbContext<TenantContext>((sp, options) =>
        {
            var tenantService = sp.GetRequiredService<ITenantService>();
            var configuration = sp.GetRequiredService<IConfiguration>();

            var builder = new Npgsql.NpgsqlConnectionStringBuilder(configuration.GetConnectionString("SharedDb"));
            builder.Database = tenantService.CurrentTenant?.DatabaseName ?? "Setup";
            
            options.UseNpgsql(builder.ConnectionString, o => 
            {
                o.UseVector();
            });
        });

        services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationContext>()
        .AddDefaultTokenProviders();
    })
    .Build();

using (var scope = host.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var context = sp.GetRequiredService<ApplicationContext>();

    Console.WriteLine("--> Ensuring database is deleted and re-migrated (Clean state)...");
    await context.Database.EnsureDeletedAsync();
    await context.Database.MigrateAsync();

    Console.WriteLine("--> Seeding Roles...");
    await RoleSeeder.SeedAsync(sp);

    Console.WriteLine("--> Seeding Master Data (Psychologists & Tenants)...");
    await MasterDataSeeder.SeedAsync(sp);
    
    Console.WriteLine("--> Seeding Tenant Data (Patients & Vectors)...");
    var userManager = sp.GetRequiredService<UserManager<User>>();
    await RawSqlSeeder.SeedTenantsAsync(sp, context, userManager);

    Console.WriteLine("==================================================");
    Console.WriteLine("MIGRATIONS AND SEEDING COMPLETED SUCCESSFULLY!");
    Console.WriteLine("==================================================");
}
