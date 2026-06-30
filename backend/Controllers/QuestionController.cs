using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindLens.Api.DTOs.Question;
using MindLens.Api.Models;
using MindLens.Api.Models.Enums;
using MindLens.Api.Filters;
using MindLens.Api.Responses;
using MindLens.Api.Services.Interfaces;

namespace MindLens.Api.Controllers;

[ApiController]
[Route("api/questions")]
//[Authorize(Roles = nameof(UserRole.Psychologist))]
public class QuestionController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionController(IQuestionService questionService)
    {
        _questionService = questionService;
    }
    
    [HttpGet]
    public async Task<ActionResult<ServiceResponse<ICollection<Question>>>> Get([FromQuery] QuestionFilters filters)
    {
        // Passing it to service
        var response = await _questionService.Get(filters);
        
        // Returning response
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServiceResponse<Question>>> GetById(Guid id)
    {
        // Passing it to service
        var response = await _questionService.GetById(id);

        // Returning response
        return StatusCode(response.StatusCode, response);
    }

    [HttpPost]
    public async Task<ActionResult<ServiceResponse>> Create(QuestionCreateDto request)
    {
        // Passing it to service
        var response = await _questionService.Create(request);
        
        // Returning response
        return StatusCode(response.StatusCode, response);
    }
}