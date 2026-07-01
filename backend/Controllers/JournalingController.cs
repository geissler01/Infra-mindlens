using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindLens.Api.DTOs.Journaling;
using MindLens.Api.Models;
using MindLens.Api.Models.Enums;
using MindLens.Api.Filters;
using MindLens.Api.Responses;
using MindLens.Api.Services.Interfaces;

namespace MindLens.Api.Controllers;

[ApiController]
[Route("api/journalings")]
//[Authorize]
public class JournalingController : ControllerBase
{
    private readonly IJournalingService _journalingService;

    public JournalingController(IJournalingService journalingService)
    {
        _journalingService = journalingService;
    }
    
    [Authorize(Roles = nameof(UserRole.Psychologist))]
    [HttpGet]
    public async Task<ActionResult<ServiceResponse<ICollection<Journaling>>>> Get([FromQuery] JournalingFilters filters)
    {
        // Passing it to service
        var response = await _journalingService.Get(filters);

        // Returning response
        return StatusCode(response.StatusCode, response);
    }

    [Authorize(Roles = nameof(UserRole.Psychologist))]
    [HttpGet("answers")]
    public async Task<ActionResult<ServiceResponse<ICollection<JournalingAnswer>>>> GetAnswers([FromQuery] JournalingAnswerFilters filters)
    {
        // Passing it to service
        var response = await _journalingService.GetAnswers(filters);

        // Returning response
        return StatusCode(response.StatusCode, response);    
    }

    [Authorize(Roles = nameof(UserRole.Psychologist))]
    [HttpGet("registers")]
    public async Task<ActionResult<ServiceResponse<ICollection<JournalingRegisterResponseDto>>>> GetRegisters(
        [FromQuery] JournalingRegisterFilters filters)
    {
        // Passing it to service
        var response = await _journalingService.GetRegisters(filters);

        // Returning response
        return StatusCode(response.StatusCode, response);
    }
    
    [Authorize(Roles = nameof(UserRole.Psychologist))]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceResponse<Journaling>>> GetById(Guid id)
    {
        // Passing it to service
        var response = await _journalingService.GetById(id);

        // Returning response
        return StatusCode(response.StatusCode, response);
    }

    [Authorize(Roles = nameof(UserRole.Psychologist))]
    [HttpGet("answers/{answerId:guid}")]
    public async Task<ActionResult<ServiceResponse<JournalingAnswer>>> GetAnswerById(Guid answerId)
    {
        // Passing it to service
        var response = await _journalingService.GetAnswerById(answerId);

        // Returning response
        return StatusCode(response.StatusCode, response);
    }

    [Authorize(Roles = nameof(UserRole.Psychologist))]
    [HttpGet("registers/{registerId:guid}")]
    public async Task<ActionResult<ServiceResponse<JournalingRegisterResponseDto>>> GetRegisterById(Guid registerId)
    {
        // Passing it to service
        var response = await _journalingService.GetRegisterById(registerId);

        // Returning response
        return StatusCode(response.StatusCode, response);
    }
    
    [Authorize(Roles = nameof(UserRole.Patient))]
    [HttpPost]
    public async Task<ActionResult<ServiceResponse>> Create(CreateJournalingDto request)
    {
        // Passing it to service
        var response = await _journalingService.Create(request);

        // Returning response
        return StatusCode(response.StatusCode, response);
    }
    
    [Authorize(Roles = nameof(UserRole.Patient))]
    [HttpPost("answers")]
    public async Task<ActionResult<ServiceResponse>> CreateAnswer(CreateJournalingAnswerDto request)
    {
        // Passing it to service
        var response = await _journalingService.CreateAnswer(request);

        // Returning response
        return StatusCode(response.StatusCode, response);
    }
    
    // Audio Management
    [Authorize(Roles = nameof(UserRole.Patient))]
    [HttpGet("s3-key")]
    public async Task<ActionResult<ServiceResponse>> GetS3Key()
    {
        // Identifying user
        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Passing it to service
        var response = await _journalingService.GetS3Key(userId);

        // Returning response
        return StatusCode(response.StatusCode, response);
    }

    [Authorize(Roles = nameof(UserRole.Patient))]
    [HttpGet("download-audio/{s3key:guid}")]
    public async Task<ActionResult<ServiceResponse>> DownloadAudio(Guid s3key)
    {
        // Passing it to service
        var response = await _journalingService.GetDownloadAudio(s3key.ToString());
        
        // Returning response
        return StatusCode(response.StatusCode, response);
    }
}