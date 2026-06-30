using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindLens.Api.DTOs.Treatment;
using MindLens.Api.Models;
using MindLens.Api.Models.Enums;
using MindLens.Api.Filters;
using MindLens.Api.Responses;
using MindLens.Api.Services.Interfaces;

namespace MindLens.Api.Controllers;

[ApiController]
[Route("api/treatments")]
//[Authorize(Roles = nameof(UserRole.Psychologist))]
public class TreatmentController : ControllerBase
{
    private readonly ITreatmentService _treatmentService;

    public TreatmentController(ITreatmentService treatmentService)
    {
        _treatmentService = treatmentService;
    }
    
    [HttpGet]
    public async Task<ActionResult<ServiceResponse<ICollection<Treatment>>>> Get([FromQuery] TreatmentFilters filters)
    {
        // Passing it to service
        var response = await _treatmentService.Get(filters);

        // Returning response
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceResponse<Treatment>>> GetById(Guid id)
    {
        // Passing it to service
        var response = await _treatmentService.GetById(id);

        // Returning response
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponse>> Create(TreatmentCreateDto request)
    {
        // Passing it to service
        var response = await _treatmentService.Create(request);

        // Returning response
        return StatusCode(response.StatusCode, response);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ServiceResponse>> Update(Guid id, TreatmentUpdateDto request) {
        // Passing it to service
        var response = await _treatmentService.Update(id, request);

        // Returning response
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id:guid}/finish")]
    public async Task<ActionResult<ServiceResponse>> Disable(Guid id)
    {
        // Passing it to Service
        var response = await _treatmentService.Finish(id);
        
        // Returning response
        return StatusCode(response.StatusCode, response);
    }
}