using Microsoft.AspNetCore.Mvc;
using MindLens.Api.DTOs.Patient;
using MindLens.Api.Filters;
using MindLens.Api.Models;
using MindLens.Api.Responses;

namespace MindLens.Api.Services.Interfaces;

public interface IPatientService
{
    public Task<ServiceResponse<ICollection<User>>> Get([FromQuery] PaginationFilters filters);
    public Task<ServiceResponse<PatientGetByIdResponse>> GetById(Guid id);
    public Task<ServiceResponse> Create(PatientCreateDto request);
    public Task<ServiceResponse> Update(Guid id, PatientUpdateDto request);
}