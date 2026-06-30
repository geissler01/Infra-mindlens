using MindLens.Api.Data;
using MindLens.Api.Responses;
using MindLens.Api.Services.Interfaces;

namespace MindLens.Api.Middlewares;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationContext sharedDb, ITenantService tenantService)
    {
        ServiceResponse response = new ServiceResponse();
        
        if(!context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantId)) 
        {
            // Routes without tenant identification
            if(
                context.Request.Path.StartsWithSegments("/api/auth") ||
                context.Request.Path.StartsWithSegments("/api/users") ||
                context.Request.Path.StartsWithSegments("/api/tenants")
                ) 
            {
                await _next(context);
                return;
            }

            response.StatusCode = 400;
            response.Message = "Missing X-Tenant-Id Header";
            response.Success = false;

            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        // Checking the tenant id is with valid format
        if (!Guid.TryParse(tenantId, out var id))
        {
            response.StatusCode = 400;
            response.Message = "Invalid tenant id";
            response.Success = false;

            await context.Response.WriteAsJsonAsync(response);
            return;
        }
        
        // Check tenant exists
        var tenant = await sharedDb.Tenants.FindAsync(id);
        
        if(tenant is null)
        {
            response.StatusCode = 404;
            response.Message = "Tenant not found";
            response.Success = false;
            
            await context.Response.WriteAsJsonAsync(response);
            return;
        }

        tenantService.CurrentTenant = tenant;
        
        await _next(context);
    }
}