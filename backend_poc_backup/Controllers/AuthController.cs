using Microsoft.AspNetCore.Mvc;
using MindLens.Api.DTOs.Auth;
using MindLens.Api.Services.Interfaces;

namespace MindLens.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    
    // <summary>Authenticates a user</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequestDto request) 
    {
        // Passing it to service
        var response = await _authService.Login(request);

        // Returning response
        return StatusCode(response.StatusCode, response);
    }

    // <summary>Registers a new user</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto request)
    {
        // Passing it to service
        var response = await _authService.Register(request);
        
        // Returning response
        return StatusCode(response.StatusCode, response);
    }
}