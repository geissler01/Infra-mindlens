using Microsoft.EntityFrameworkCore;
using MindLens.Api.Data;
using MindLens.Api.DTOs.Question;
using MindLens.Api.Filters;
using MindLens.Api.Models;
using MindLens.Api.Responses;
using MindLens.Api.Services.Interfaces;

namespace MindLens.Api.Services;

public class QuestionService : IQuestionService
{
    private readonly TenantContext _tenantContext;

    public QuestionService(TenantContext tenantContext)
    {
        _tenantContext = tenantContext;
    }
    
    public async Task<ServiceResponse<ICollection<Question>>> Get(QuestionFilters filters)
    {
        ServiceResponse<ICollection<Question>> response = new ServiceResponse<ICollection<Question>>();
        
        // Creating basic query
        var query = _tenantContext.Questions.AsQueryable();
        
        // Aplying filters
        if(!string.IsNullOrEmpty(filters.Question)) query = query.Where(q => q.Message.Contains(filters.Question));
        if(filters.Type is not null) query = query.Where(q => q.Type == filters.Type);
        
        // Making query
        query = query.Skip((filters.Page - 1) * filters.PageSize).Take(filters.PageSize);

        var questions = await query.ToListAsync();
        
        // Returning response
        response.StatusCode = 200;
        response.Message = "Questions found!";
        response.Success = true;
        response.Data = questions;

        return response;
    }

    public async Task<ServiceResponse<Question>> GetById(Guid id)
    {
        ServiceResponse<Question> response = new ServiceResponse<Question>();
        
        // Searching
        var question = await _tenantContext.Questions.FindAsync(id);

        // Checking it exists
        if (question is null)
        {
            response.StatusCode = 404;
            response.Message = "Question not found!";
            response.Success = false;

            return response;
        }

        // Returning response
        response.StatusCode = 200;
        response.Message = "Question found!";
        response.Success = true;
        response.Data = question;
        
        return response;
    }

    public async Task<ServiceResponse> Create(QuestionCreateDto request)
    {
        ServiceResponse response = new ServiceResponse();
        
        // Check there isn't any same question
        var questionFound = await _tenantContext.Questions.AnyAsync(q => q.Message.ToLower() == request.Question.ToLower());

        if (questionFound)
        {
            response.StatusCode = 409;
            response.Message = "Question already exists!";
            response.Success = false;
            
            return response;
        }
        
        // Creating question
        Question question = new Question
        {
            Message = request.Question,
            Type = request.Type,
        };

        await _tenantContext.Questions.AddAsync(question);
        
        // Saving in db
        await  _tenantContext.SaveChangesAsync();

        // Returning response
        response.StatusCode = 201;
        response.Message = "Question created successfully";
        response.Success = true;

        return response;
    }
}