using Microsoft.EntityFrameworkCore;
using MindLens.Api.Data;
using MindLens.Api.Services.Interfaces;
using MindLens.Api.Responses;
using MindLens.Api.Models;
using MindLens.Api.Filters;
using MindLens.Api.Models.Enums;
using MindLens.Api.DTOs.Treatment;

namespace MindLens.Api.Services;

public class TreatmentService : ITreatmentService
{
    private readonly TenantContext _tenantContext;
    private readonly ApplicationContext _applicationContext;

    public TreatmentService(TenantContext tenantContext, ApplicationContext applicationContext)
    {
        _tenantContext = tenantContext;
        _applicationContext = applicationContext;
    }
    
    public async Task<ServiceResponse<ICollection<Treatment>>> Get(TreatmentFilters filters)
    {
        ServiceResponse<ICollection<Treatment>> response = new ServiceResponse<ICollection<Treatment>>();
        
        // Creating basic query
        var query = _tenantContext.Treatments.AsQueryable();

        // Applying filters
        if (filters.PatientId is not null) query = query.Where(t => t.PatientId == filters.PatientId);
        if (filters.SessionDay is not null) query = query.Where(t => t.SessionDay == filters.SessionDay);
        if (filters.State is not null) query = query.Where(t => t.State == filters.State);

        // Making query
        query = query.Skip((filters.Page - 1) * filters.PageSize).Take(filters.PageSize);
        var users = await query.ToListAsync();

        // Returning response
        response.StatusCode = 200;
        response.Message = "Treatments found";
        response.Success = true;
        response.Data = users;

        return response;
    }
    public async Task<ServiceResponse<Treatment>> GetById(Guid id)
    {
        ServiceResponse<Treatment> response = new ServiceResponse<Treatment>();
        
        // Searching
        var treatment = await _tenantContext.Treatments.FindAsync(id);

        // Checking it exists
        if(treatment is null) 
        {
            response.StatusCode = 404;
            response.Message = "Treatment not found";
            response.Success = false;

            return response;
        }

        // Returning response
        response.StatusCode = 200;
        response.Message = "Treatment found!";
        response.Success = true;
        response.Data = treatment;
        
        return response;
    }

    public async Task<ServiceResponse> Create(TreatmentCreateDto request)
    {
        ServiceResponse response = new ServiceResponse();
        
        // Checking patient exists
        var patient = await _tenantContext.Patients.Include(p => p.Treatments).FirstOrDefaultAsync(p => p.Id == request.PatientId);

        if (patient is null)
        {
            response.StatusCode = 400;
            response.Message = "Patient doesn't exist";
            response.Success = false;
            
            return response;
        }
        
        // Checking patient hasn't any active treatment
        var userTreatments = patient.Treatments.Any(t => t.State == TreatmentState.InProcess);

        if (userTreatments)
        {
            response.StatusCode = 409;
            response.Message = "Patient has an active treatment";
            response.Success = false;

            return response;
        }
        
        // Creating treatment
        Treatment newTreatment = new Treatment
        {
            PatientId = request.PatientId,
            StartedAt = DateOnly.FromDateTime(DateTime.Today),
            SessionDay = request.SessionDay
        };

        await _tenantContext.Treatments.AddAsync(newTreatment);
        
        // Saving changes
        await _tenantContext.SaveChangesAsync();

        // Returning response
        response.StatusCode = 201;
        response.Message = "Treatment created successfully";
        response.Success = true;
        
        return response;
    }
    public async Task<ServiceResponse> Update(Guid id, TreatmentUpdateDto request)
    {
        ServiceResponse response = new ServiceResponse();
        
        // Searching Treatment
        var treatment = await _tenantContext.Treatments.FindAsync(id);

        // Checking it exists
        if (treatment is null)
        {
            response.StatusCode = 404;
            response.Message = "Treatment not found";
            response.Success = false;

            return response;
        }

        // Updating
        if (request.SessionDay is not null) treatment.SessionDay = request.SessionDay.Value;
        if (request.State is not null) treatment.State = request.State.Value;

        // Saving Changes
        await _tenantContext.SaveChangesAsync();

        // Returning response
        response.StatusCode = 200;
        response.Message = "Treatment updated";
        response.Success = true;

        return response;
    }

    public async Task<ServiceResponse> Finish(Guid id)
    {
        ServiceResponse response = new ServiceResponse();
        
        // Searching treatment
        var treatment = await _tenantContext.Treatments.FindAsync(id);
        
        // Checking it exists
        if(treatment is null)
        {
            response.StatusCode = 404;
            response.Message = "Treatment not found";
            response.Success = false;
            
            return response;
        }
        
        // Finishing treatment
        treatment.State = TreatmentState.Finished;

        // Saving changes
        await _tenantContext.SaveChangesAsync();

        // Returning response
        response.StatusCode = 200;
        response.Message = "Treatment Finished";
        response.Success = true;
        
        return response;
    }

    // Treatment Questions
    public async Task<ServiceResponse<ICollection<TreatmentQuestion>>> GetTreatmentQuestions(Guid treatmentId)
    {
        ServiceResponse<ICollection<TreatmentQuestion>> response = new ServiceResponse<ICollection<TreatmentQuestion>>();
        
        // Check treatment exists
        var treatment = await _tenantContext.Treatments.Include(t => t .TreatmentQuestions).FirstOrDefaultAsync(t => t.Id == treatmentId);

        if (treatment is null)
        {
            response.StatusCode = 404;
            response.Message = "Treatment not found";
            response.Success = false;
            
            return response;
        }
        
        // Return response
        response.StatusCode = 200;
        response.Message = "Treatment questions found";
        response.Success = true;
        response.Data =  treatment.TreatmentQuestions;
        
        return response;
    }
    public async Task<ServiceResponse> AssignQuestion(Guid id, Guid questionId)
    {
        ServiceResponse response = new ServiceResponse();
        
        // Check the treatment exists
        var treatment = await _tenantContext.Treatments.Include(t => t.TreatmentQuestions).FirstOrDefaultAsync(t => t.Id == id);

        if (treatment is null)
        {
            response.StatusCode = 404;
            response.Message = "Treatment not found";
            response.Success = false;

            return response;
        }
        
        // Check the question exists
        var question = await _tenantContext.Questions.FindAsync(questionId);

        if (question is null)
        {
            response.StatusCode = 404;
            response.Message = "Question not found";
            response.Success = false;
            
            return response;
        }
        
        // Check the patient hasn't this question assigned yet
        if (treatment.TreatmentQuestions.Any(t => t.QuestionId == question.Id))
        {
            response.StatusCode = 409;
            response.Message = "Question has already been assigned";
            response.Success = false;
            
            return response;
        }
        
        // Assign question to treatment
        TreatmentQuestion newTreatmentQuestion = new TreatmentQuestion
        {
            QuestionId = question.Id,
            TreatmentId = treatment.Id
        };

        await _tenantContext.TreatmentQuestions.AddAsync(newTreatmentQuestion);

        // Save changes in db
        await _tenantContext.SaveChangesAsync();

        // Return response
        response.StatusCode = 201;
        response.Message = "Treatment question assigned successfully";
        response.Success = true;
        
        return response;
    }
}