using MindLens.Api.Models;
using MindLens.Api.Responses;
using MindLens.Api.Filters;
using MindLens.Api.DTOs;
using MindLens.Api.DTOs.Question;

namespace MindLens.Api.Services.Interfaces;

public interface IQuestionService
{
    public Task<ServiceResponse<ICollection<Question>>> Get(QuestionFilters filters);
    public Task<ServiceResponse<Question>> GetById(Guid id);
    
    public Task<ServiceResponse> Create(QuestionCreateDto request);
}