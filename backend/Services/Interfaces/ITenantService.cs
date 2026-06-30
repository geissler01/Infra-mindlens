using Microsoft.EntityFrameworkCore.ChangeTracking;
using MindLens.Api.Models;
using MindLens.Api.Responses;

namespace MindLens.Api.Services.Interfaces;

public interface ITenantService
{
    public Tenant? CurrentTenant { get; set; }
    public Task<ServiceResponse<IEnumerable<Tenant>>> Get();
    public Task<ServiceResponse<Tenant>> GetById(Guid id);
    public Task<ServiceResponse<EntityEntry<Tenant>>> Create(Guid psychologistId);
    public Task<ServiceResponse> ProvideTenant(Guid id);
    public Task CreateDatabase(string databaseName);
    public Task RunMigrations(string databaseName);
    public Task ConnectTenant();
    public Task<ServiceResponse> Enable(Guid id);
    public Task<ServiceResponse> Disable(Guid id);
}