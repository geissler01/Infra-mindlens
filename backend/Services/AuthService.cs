using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using MindLens.Api.DTOs.Auth;
using MindLens.Api.Models;
using MindLens.Api.Models.Enums;
using MindLens.Api.Responses;
using MindLens.Api.Services.Interfaces;

namespace MindLens.Api.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IJwtService _jwtService;

    public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
    }
    
    public async Task<ServiceResponse<string>> Login(LoginRequestDto request)
    {
        ServiceResponse<string> response = new ServiceResponse<string>();
        
        var user = await _userManager.FindByEmailAsync(request.Email);
        
        // Checking the user with that email exists
        if(user is null)
        {
            response.StatusCode = 400;
            response.Message = "Invalid email or password";
            response.Success = false;

            return response;
        }
        
        // Checking the credentials matches
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            response.StatusCode = 400;
            response.Message = "Invalid email or password";
            response.Success = false;

            return response;
        }

        // Generating token and returning response        
        var token = await _jwtService.GenerateTokenAsync(user);

        response.StatusCode = 200;
        response.Data = token;
        response.Success = true;
        response.Message = "Successfully logged in";

        return response;
    }

    public async Task<ServiceResponse<string>> Register(RegisterRequestDto request)
    {
        ServiceResponse<string> response = new ServiceResponse<string>();
        
        // Checking the email isn't registered yet
        var userExists = await _userManager.FindByEmailAsync(request.Email);

        if (userExists is not null)
        {
            response.StatusCode = 400;
            response.Message = "This email isn't available";
            response.Success = false;
        }
        
        // Creating User
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            UserName = request.Email
        };
        
        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            response.StatusCode = 400;
            response.Message = "User couldn't be registered";
            response.Success = false;

            foreach (var error in result.Errors)
            {
                response.Errors.Add(error.Description);
            }
            
            return response;
        }
        
        // Assigning default role
        await _userManager.AddToRoleAsync(user, UserRole.Patient.ToString());
        
        // Generating token
        var token = await _jwtService.GenerateTokenAsync(user);
        
        // Returning response
        response.StatusCode = 201;
        response.Message = "Successfully registered";
        response.Success = true;
        response.Data = token;

        return response;
    }
}