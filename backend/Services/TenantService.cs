using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MindLens.Api.Data;
using MindLens.Api.Models;
using MindLens.Api.Models.Enums;
using MindLens.Api.Responses;
using MindLens.Api.Services.Interfaces;
using Npgsql;

namespace MindLens.Api.Services;

public class TenantService : ITenantService
{
    private readonly ApplicationContext _sharedDb;
    private readonly IConfiguration _configuration;
    
    public Tenant? CurrentTenant { get; set; }
    
    public TenantService(ApplicationContext sharedDb, IConfiguration configuration) 
    {
        _sharedDb = sharedDb;
        _configuration = configuration;
    }

    public async Task<ServiceResponse<IEnumerable<Tenant>>> Get()
    {
        ServiceResponse<IEnumerable<Tenant>> response = new ServiceResponse<IEnumerable<Tenant>>();
        
        // Get Tenants
        var tenants = await _sharedDb.Tenants.ToListAsync();

        // Return response
        response.Data = tenants;
        response.StatusCode = 200;
        response.Message = "Tenants found";
        response.Success = true;
        
        return response;
    }
    
    public async Task<ServiceResponse<Tenant>> GetById(Guid id)
    {
        ServiceResponse<Tenant> response = new ServiceResponse<Tenant>();
        
        // Find tenant
        var tenant = await _sharedDb.Tenants.FindAsync(id);

        // Check it exists
        if (tenant is null)
        {
            response.StatusCode = 404;
            response.Message = "Tenant Not Found";
            response.Success = false;
            
            return response;
        }

        // Return response
        response.Data = tenant;
        response.StatusCode = 200;
        response.Message = "Tenant found";
        response.Success = true;

        return response;
    }
    
    public async Task<ServiceResponse<EntityEntry<Tenant>>> Create(Guid psychologistId) 
    {
        ServiceResponse<EntityEntry<Tenant>> response = new ServiceResponse<EntityEntry<Tenant>>();
        
        // Verify the psychologist hasn't any active tenant
        var tenantFound = await _sharedDb.Tenants.AnyAsync(t => t.PsychologistId == psychologistId);

        if (tenantFound)
        {
            response.StatusCode = 409;
            response.Message = "This psychologist already has a tenant";
            response.Success = false;

            return response;
        }
        
        // Create Tenant in Shared Db
        var newTenant = await _sharedDb.Tenants.AddAsync(new Tenant()
        {
            PsychologistId = psychologistId,
            State = TenantState.Pending,
            DatabaseName = $"tenant_{psychologistId}"
        });

        response.StatusCode = 201;
        response.Message = "Tenant Created Successfully!";
        response.Success = true;
        response.Data = newTenant;

        await _sharedDb.SaveChangesAsync();
        return response;
    }
    
    public async Task<ServiceResponse> ProvideTenant(Guid id)
    {
        ServiceResponse response = new ServiceResponse();
        
        // Verify the psychologist hasn't any active tenant
        var tenant = await _sharedDb.Tenants.FindAsync(id);

        if (tenant is null)
        {
            response.StatusCode = 404;
            response.Message = "This tenant doesn't exists";
            response.Success = false;

            return response;
        }
        
        // Checking tenant isn't available yet
        if (tenant.State == TenantState.Available)
        {
            response.StatusCode = 400;
            response.Message = "This tenant is already available";
            response.Success = false;
            
            return response;
        }

        // Create Tenant DB
        await CreateDatabase(tenant.DatabaseName);
        
        // Run Migrations
        await RunMigrations(tenant.DatabaseName);

        tenant.State = TenantState.Available;
        
        response.StatusCode = 201;
        response.Success = true;
        response.Message = "Tenant DB Created Successfully";

        await _sharedDb.SaveChangesAsync();
        return response;
    }

    public async Task CreateDatabase(string databaseName)
    {
        var adminConnection = _configuration.GetConnectionString("SharedDb");
                
        await using var connection = new NpgsqlConnection(adminConnection);
        
        await connection.OpenAsync();
        
        // Check if DB already exists
        await using var commandDbExists = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @name", connection);
        commandDbExists.Parameters.AddWithValue("name", databaseName);
        
        var dbFound = await commandDbExists.ExecuteScalarAsync();

        if (dbFound is not null) return;
        
        // Create DB
        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE \"{databaseName}\"";
        
        await command.ExecuteNonQueryAsync();
        
        // Install Vector Extension
        var builder = new NpgsqlConnectionStringBuilder(_configuration.GetConnectionString("SharedDb"));
        builder.Database = databaseName;
        
        await using (var tenantConnection = new NpgsqlConnection(builder.ConnectionString)) 
        {
            await tenantConnection.OpenAsync();

            await using var commandExtension = tenantConnection.CreateCommand();
            commandExtension.CommandText = "CREATE EXTENSION IF NOT EXISTS vector";
            await commandExtension.ExecuteNonQueryAsync();
        }
    }
    
    public async Task RunMigrations(string databaseName)
    {
        // Connect to DB
        var builder = new NpgsqlConnectionStringBuilder(_configuration.GetConnectionString("SharedDb"));

        builder.Database = databaseName;

        var options = new DbContextOptionsBuilder<TenantContext>()
                .UseNpgsql(builder.ConnectionString, o => o.UseVector())
                .Options;

        await using var tenantDb = new TenantContext(options);
        
        // Run Migrations
        await tenantDb.Database.MigrateAsync();
    }

    public async Task ConnectTenant()
    {
        
    }
    
    public async Task<ServiceResponse> Enable(Guid id) 
    {
        ServiceResponse response = new ServiceResponse();
                
        // Find tenant
        var tenant = await _sharedDb.Tenants.FindAsync(id);
                
        // Check it exists
        if (tenant is null)
        {
            response.StatusCode = 404;
            response.Message = "Tenant Not Found.";
            response.Success = false;
        
            return response;
        }
                
        // Check isn't enabled yet
        if(tenant.State != TenantState.Disabled) 
        {
            response.StatusCode = 400;
            response.Message = "Tenant could not be enabled";
            response.Success = false;
                    
            return response; 
        }
                
        // Enable
        tenant.State = TenantState.Available;
                
        // Update Tenant 
        await _sharedDb.SaveChangesAsync();
        
        // Return response
        response.StatusCode = 200;
        response.Message = "Enabled with success";
        response.Success = true;
        
        return response;
    }
    
    public async Task<ServiceResponse> Disable(Guid id) 
    {
        ServiceResponse response = new ServiceResponse();
        
        // Find tenant
        var tenant = await _sharedDb.Tenants.FindAsync(id);
        
        // Check it exists
        if (tenant is null)
        {
            response.StatusCode = 404;
            response.Message = "Tenant Not Found.";
            response.Success = false;

            return response;
        }
        
        // Check isn't disabled yet
        if(tenant.State != TenantState.Available) 
        {
            response.StatusCode = 400;
            response.Message = "Tenant could not be disabled";
            response.Success = false;
            
            return response;
        }
        
        // Disable
        tenant.State = TenantState.Disabled;

        // Update Tenant
        await _sharedDb.SaveChangesAsync();
        
        // Return response
        response.StatusCode = 200;
        response.Message = "Disabled with success";
        response.Success = true;

        return response;
    }
}