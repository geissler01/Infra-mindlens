using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MindLens.Api.Data;
using MindLens.Api.DTOs.User;
using MindLens.Api.Filters;
using MindLens.Api.Models;
using MindLens.Api.Models.Enums;
using MindLens.Api.Responses;
using MindLens.Api.Services.Interfaces;

namespace MindLens.Api.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationContext _context;
    private readonly ITenantService _tenantService;

    public UserService(UserManager<User> userManager, ApplicationContext context, ITenantService tenantService)
    {
        _userManager = userManager;
        _context = context;
        _tenantService = tenantService;
    }

    public async Task<ServiceResponse> Store(AddUserDto request)
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
            UserName = request.Email,
            Role = request.Role
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
        await _userManager.AddToRoleAsync(user, request.Role.ToString());

        // Creating Tenant Entity if is psychologist
        if(request.Role == UserRole.Psychologist) 
        {
            await _tenantService.Create(user.Id);
        }
        
        // Returning response
        response.StatusCode = 201;
        response.Message = "Successfully registered";
        response.Success = true;

        return response;
    }

    public async Task<ServiceResponse<ICollection<User>>> Get(UserFilters filters)
    {
        ServiceResponse<ICollection<User>> response = new ServiceResponse<ICollection<User>>();
        var query = _context.Users.AsQueryable();

        // Aplying filters
        if (!string.IsNullOrWhiteSpace(filters.Role.ToString()))
        {
            query = query.Where(u => u.Role == filters.Role);
        }

        // Aplying page and quantity limit
        query = query.Skip((filters.Page - 1) * filters.PageSize).Take(filters.PageSize);

        // Making query & Returning response
        var users = await query.ToListAsync();

        response.StatusCode = 200;
        response.Message = "Users found!";
        response.Success = true;
        response.Data = users;

        return response;
    }

    public async Task<ServiceResponse<User>> GetById(Guid id)
    {
        ServiceResponse<User> response = new ServiceResponse<User>();

        // Searching User
        var user = await _userManager.FindByIdAsync(id.ToString());

        // Checking it exists
        if (user is null)
        {
            response.StatusCode = 404;
            response.Message = "User not found";
            response.Success = false;

            return response;
        }

        // Returning response
        response.StatusCode = 200;
        response.Message = "User Found!";
        response.Success = true;
        response.Data = user;

        return response;
    }

    public async Task<ServiceResponse> Update(Guid id, UpdateUserDto request)
    {
        ServiceResponse response = new ServiceResponse();
        var user = await _userManager.FindByIdAsync(id.ToString());

        // Checking user exists
        if (user is null)
        {
            response.StatusCode = 404;
            response.Message = "User not found";
            response.Success = false;

            return response;
        }

        // Updating incoming data
        if (request.FirstName is not null) user.FirstName = request.FirstName;
        if (request.LastName is not null) user.LastName = request.LastName;
        if (request.Email is not null)
        {
            var emailResult = await _userManager.SetEmailAsync(user, request.Email);
            var usernameResult = await _userManager.SetUserNameAsync(user, request.Email);

            if (!emailResult.Succeeded || !usernameResult.Succeeded)
            {
                response.StatusCode = 400;
                response.Message = "Failed the update of user";
                response.Success = false;

                if (!emailResult.Succeeded)
                {
                    foreach (var error in emailResult.Errors)
                    {
                        response.Errors.Add(error.Description);
                    }
                }

                if (!usernameResult.Succeeded)
                {
                    foreach (var error in usernameResult.Errors)
                    {
                        response.Errors.Add(error.Description);
                    }
                }

                return response;
            }
        }
        if (request.Role is not null && request.Role != user.Role)
        {
            var quitRole = await _userManager.RemoveFromRoleAsync(user, request.Role.ToString());
            var addRole = await _userManager.AddToRoleAsync(user, request.Role.ToString());

            user.Role = request.Role.Value;
            
            if (!quitRole.Succeeded || !addRole.Succeeded)
            {
                response.StatusCode = 400;
                response.Message = "Failed the update of user";
                response.Success = false;

                if (!quitRole.Succeeded)
                {
                    foreach (var error in quitRole.Errors)
                    {
                        response.Errors.Add(error.Description);
                    }
                }

                if (!addRole.Succeeded)
                {
                    foreach (var error in addRole.Errors)
                    {
                        response.Errors.Add(error.Description);
                    }
                }

                return response;
            }
        }

        // Updating
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            response.StatusCode = 400;
            response.Message = "Failed the update of user";
            response.Success = false;

            foreach (var error in result.Errors)
            {
                response.Errors.Add(error.Description);
            }

            return response;
        }

        // Returning response
        response.StatusCode = 200;
        response.Message = "User updated successfully";
        response.Success = true;

        return response;
    }
}