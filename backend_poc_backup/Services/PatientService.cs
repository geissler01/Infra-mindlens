using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MindLens.Api.Data;
using MindLens.Api.Models;
using MindLens.Api.Responses;
using MindLens.Api.Filters;
using MindLens.Api.DTOs.Patient;
using MindLens.Api.Services.Interfaces;

namespace MindLens.Api.Services;

public class PatientService : IPatientService
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationContext _sharedContext;
    private readonly TenantContext _tenantContext;
    
    public PatientService(UserManager<User> userManager,ApplicationContext sharedContext, TenantContext tenantContext) 
    {
        _userManager = userManager;
        _sharedContext = sharedContext;
        _tenantContext = tenantContext;
    }

    public async Task<ServiceResponse<ICollection<User>>> Get(PaginationFilters filters)
    {
        ServiceResponse<ICollection<User>> response = new ServiceResponse<ICollection<User>>();
        
        // Getting all users
        var patientUserIds = await _tenantContext.Patients.Select(p => p.GlobalUserId).ToListAsync();
        var query = _sharedContext.Users.Where(u => patientUserIds.Contains(u.Id));
        
        // Aplying filters
        query = query.Skip((filters.Page - 1) * filters.PageSize).Take(filters.PageSize);
        
        // Doing query
        var users = await query.ToListAsync();

        response.StatusCode = 200;
        response.Message = "Users found";
        response.Success = true;
        response.Data = users;

        return response;
    }
    
    public async Task<ServiceResponse<PatientGetByIdResponse>> GetById(Guid id)
    {
        ServiceResponse<PatientGetByIdResponse> response = new ServiceResponse<PatientGetByIdResponse>();
        
        // Searching patient
        var patient = await _tenantContext.Patients.FindAsync(id);
        
        // Checking patient exists
        if (patient is null)
        {
            response.StatusCode = 404;
            response.Message = "Patient not found.";
            response.Success = false;

            return response;
        }
        
        // Searching user
        var user = await _sharedContext.Users.FirstOrDefaultAsync(u => u.Id == patient.GlobalUserId);

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
        response.Message = "User found!";
        response.Success = true;
        response.Data = new PatientGetByIdResponse
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = patient.Phone,
            EmergencyPhone = patient.EmergencyPhone,
            Address = patient.Address,
            AgeRange = patient.AgeRange,
            RelationshipStatus = patient.RelationshipStatus,
            Occupation = patient.Occupation,
            LivingSituation = patient.LivingSituation,
            PrimaryGoal = patient.PrimaryGoal,
            HasPreviousTherapy = patient.HasPreviousTherapy,
            CreatedAt = user.CreatedAt
        };
        
        return response;
    }
    
    public async Task<ServiceResponse> Create(PatientCreateDto request)
    {
        ServiceResponse response = new ServiceResponse();
        
        // Check if user already exists
        var user = _sharedContext.Users.FirstOrDefault(u => u.Email == request.Email);
        
        // User isn't created
        if(user is null)
        {
            // Create user
            user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email
            };

            var result = await _userManager.CreateAsync(user, $"MindLens12345-{request.Email}");

            // Check the user was created
            if (!result.Succeeded)
            {
                response.StatusCode = 400;
                response.Message = "User couldn't be registered";
                response.Success = false;

                foreach (var error in result.Errors)
                {
                    response.Errors.Add(error.Description);
                }
            }
            
            // Assigning default role
            await _userManager.AddToRoleAsync(user, "Patient");
        }
        
        // Check patient isn't already registered
        var patientFound = await _tenantContext.Patients.AnyAsync(p => p.GlobalUserId == user.Id);
        
        if(patientFound)
        {
            response.StatusCode = 400;
            response.Message = "Patient is already registered";
            response.Success = false;

            return response;
        }
        
        // Create patient
        Patient newPatient = new Patient
        {
            GlobalUserId = user.Id,
            Phone = request.Phone,
            EmergencyPhone = request.EmergencyPhone,
            Address = request.Address,
            AgeRange = request.AgeRange,
            RelationshipStatus = request.RelationshipStatus,
            Occupation = request.Occupation,
            LivingSituation = request.LivingSituation,
            PrimaryGoal = request.PrimaryGoal,
            HasPreviousTherapy = request.HasPreviousTherapy,
        };
        
        _tenantContext.Patients.Add(newPatient);
        await _tenantContext.SaveChangesAsync();
        
        // Return response
        response.StatusCode = 201;
        response.Message = "Patient Created. Default Password must be MindLens12345-USER_EMAIL";
        response.Success = true;

        return response;
    }
    
    public async Task<ServiceResponse> Update(Guid id, PatientUpdateDto request) 
    {
        ServiceResponse response = new ServiceResponse();
        
        // Check the patient exists
        var patient = await _tenantContext.Patients.FindAsync(id);

        if (patient is null)
        {
            response.StatusCode = 404;
            response.Message = "Patient not found";
            response.Success = false;
            
            return response;
        }

        // Update patient
        if(!string.IsNullOrEmpty(request.Phone)) patient.Phone = request.Phone;
        if(!string.IsNullOrEmpty(request.EmergencyPhone)) patient.EmergencyPhone = request.EmergencyPhone;
        if(!string.IsNullOrEmpty(request.Address)) patient.Address = request.Address;
        if(!string.IsNullOrEmpty(request.AgeRange)) patient.AgeRange = request.AgeRange;
        if(!string.IsNullOrEmpty(request.RelationshipStatus)) patient.RelationshipStatus = request.RelationshipStatus;
        if(!string.IsNullOrEmpty(request.Occupation)) patient.Occupation = request.Occupation;
        if(!string.IsNullOrEmpty(request.LivingSituation)) patient.LivingSituation = request.LivingSituation;
        if(!string.IsNullOrEmpty(request.PrimaryGoal)) patient.PrimaryGoal = request.PrimaryGoal;

        // Save changes
        await _tenantContext.SaveChangesAsync();
        
        // Return response
        response.StatusCode = 200;
        response.Message = "Patient Updated";
        response.Success = true;
        
        return response;
    }
}