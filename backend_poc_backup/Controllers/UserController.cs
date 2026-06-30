using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindLens.Api.DTOs.User;
using MindLens.Api.Filters;
using MindLens.Api.Models.Enums;
using MindLens.Api.Services.Interfaces;

namespace MindLens.Api.Controllers;

[ApiController]
[Route("api/users")]
//[Authorize(Roles = nameof(UserRole.Admin))]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] UserFilters filters)
    {
        var response = await _userService.Get(filters);
        
        // Returning response
        return StatusCode(response.StatusCode, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _userService.GetById(id);
        
        // Returning response
        return StatusCode(response.StatusCode, response);
    }
    
    // <summary>Adds a new user</summary>
    [HttpPost]
    // Authorize
    public async Task<IActionResult> Create(AddUserDto request) 
    {
        // Passing to service
        var response = await _userService.Store(request);
        
        // Returning response
        return StatusCode(response.StatusCode, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateUserDto request)
    {
        var response = await _userService.Update(id, request);
        
        // Returning response
        return StatusCode(response.StatusCode, response);
    }
}