using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindLens.Api.DTOs.Patient;
using MindLens.Api.Models;
using MindLens.Api.Models.Enums;
using MindLens.Api.Filters;
using MindLens.Api.Responses;
using MindLens.Api.Services.Interfaces;

namespace MindLens.Api.Controllers;

[ApiController]
[Route("api/patients")]
//[Authorize(Roles = nameof(UserRole.Psychologist))]
public class PatientController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientController(IPatientService patientService)
    {
        _patientService = patientService;
    }
    
    [HttpGet]
    public async Task<ActionResult<ServiceResponse<ICollection<Patient>>>> Get([FromQuery] PaginationFilters filters)
    {
        // Passing it to service
        var response = await _patientService.Get(filters);

        // Returning response
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceResponse<Patient>>> GetById(Guid id)
    {
        // Passing it to service
        var response = await _patientService.GetById(id);
        
        // Returning response
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponse>> Create(PatientCreateDto request)
    {
        // Passing it to service
        var response = await _patientService.Create(request);
        
        // Returning response
        return StatusCode(response.StatusCode, response);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ServiceResponse>> Update(Guid id, PatientUpdateDto request) {
        // Passing it to service
        var response = await _patientService.Update(id, request);
        
        // Returning response
        return StatusCode(response.StatusCode, response);
    }
}

/*
 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindLens.Api.DTOs.Patient;
using MindLens.Api.Models;
using MindLens.Api.Models.Enums;
using MindLens.Api.Filters;
using MindLens.Api.Responses;
using MindLens.Api.Services.Interfaces;

namespace MindLens.Api.Controllers;

[ApiController]
[Route("api/patients")]
//[Authorize(Roles = nameof(UserRole.Psychologist))]
public class PatientController : ControllerBase
{
    private readaonly IPatientService _patientService;

    public PatientController(IPatientService patientService)
    {
        _patientService = patientService;
    }
    
    [HttpGet]
    public async Task<ActionResult<ServiceResponse<ICollection<Patient>>>> Get([FromQuery] PaginationFilters filters)
    {
        // Passing it to service
        
        // Returning response
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceResponse<Patient>>> GetById(Guid id)
    {
        // Passing it to service
        
        // Returning response
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponse>> Create(PatientCreateDto request)
    {
        // Passing it to service
        
        // Returning response
    }
    
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ServiceResponse>> Update(PatientUpdateDto request) {
        // Passing it to service
        
        // Returning response
    }
}

*/