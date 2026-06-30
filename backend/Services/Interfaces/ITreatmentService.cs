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
}