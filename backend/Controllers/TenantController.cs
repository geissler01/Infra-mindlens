using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindLens.Api.Models.Enums;
using MindLens.Api.Responses;
using MindLens.Api.Services.Interfaces;

namespace MindLens.Api.Controllers;

[ApiController]
[Route("api/tenants")]
//[Authorize(Roles = nameof(UserRole.Admin))]
public class TenantController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }
    
    [HttpGet]
    public async Task<ActionResult<ServiceResponse>> Get()
    {
        // Request to service
        var response = await _tenantService.Get();

        // Return response
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceResponse>> GetById(Guid id)
    {
        // Request to service
        var response = await _tenantService.GetById(id);

        // Return response
        return StatusCode(response.StatusCode, response);
    }
    
    [HttpGet("{id:guid}/enable")]
    public async Task<ActionResult<ServiceResponse>> Enable(Guid id)
    {
        // Request to service
        var response = await _tenantService.Enable(id);
        
        // Return response
        return StatusCode(response.StatusCode, response);
    }
    
    [HttpGet("{id:guid}/disable")]
    public async Task<ActionResult<ServiceResponse>> Disable(Guid id) 
    {
        // Request to service
        var response = await _tenantService.Disable(id);
        
        // Return response
        return StatusCode(response.StatusCode, response);
    }
    
    [HttpGet("{id:guid}/provide")]
    public async Task<ActionResult<ServiceResponse>> ProvideTenant(Guid id) 
    {
        // Request to service
        var response = await _tenantService.ProvideTenant(id);

        // Return response
        return StatusCode(response.StatusCode, response);
    }
}