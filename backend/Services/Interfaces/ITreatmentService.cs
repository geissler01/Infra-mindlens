using MindLens.Api.DTOs.Treatment;
using MindLens.Api.Filters;
using MindLens.Api.Responses;
using MindLens.Api.Models;

namespace MindLens.Api.Services.Interfaces;

public interface ITreatmentService
{
    public Task<ServiceResponse<ICollection<Treatment>>> Get(TreatmentFilters filters);
    public Task<ServiceResponse<Treatment>> GetById(Guid id);
    
    public Task<ServiceResponse> Create(TreatmentCreateDto request);
    public Task<ServiceResponse> Update(Guid id, TreatmentUpdateDto request);
    
    public Task<ServiceResponse> Finish(Guid id);
    
    public Task<ServiceResponse<ICollection<TreatmentQuestion>>> GetTreatmentQuestions(Guid treatmentId);
    public Task<ServiceResponse> AssignQuestion(Guid id, Guid questionId);
    //public Task<ServiceResponse> UnassignQuestion(Guid id, Guid questionId);
}