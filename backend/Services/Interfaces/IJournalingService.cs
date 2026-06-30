using MindLens.Api.Models;
using MindLens.Api.Responses;
using MindLens.Api.Filters;
using MindLens.Api.DTOs.Journaling;

namespace MindLens.Api.Services.Interfaces;

public interface IJournalingService
{
    public Task<ServiceResponse<ICollection<Journaling>>> Get(JournalingFilters filters);
    public Task<ServiceResponse<ICollection<JournalingAnswer>>> GetAnswers(JournalingAnswerFilters filters);
    public Task<ServiceResponse<ICollection<JournalingRegisterResponseDto>>> GetRegisters(JournalingRegisterFilters filters);

    public Task<ServiceResponse<Journaling>> GetById(Guid id);
    public Task<ServiceResponse<JournalingAnswer>> GetAnswerById(Guid answerId);
    public Task<ServiceResponse<JournalingRegisterResponseDto>> GetRegisterById(Guid registerId);

    public Task<ServiceResponse> Create(CreateJournalingDto request);
    public Task<ServiceResponse> CreateAnswer(CreateJournalingAnswerDto request);
    
    // Audio Management
    public Task<ServiceResponse<JournalingUploadAudioResponse>> GetS3Key(Guid patientId);
    public Task<ServiceResponse<JournalingDownloadAudioResponse>> GetDownloadAudio(string s3key);
}