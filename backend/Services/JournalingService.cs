using Microsoft.EntityFrameworkCore;
using MindLens.Api.Data;
using MindLens.Api.Services.Interfaces;
using MindLens.Api.Responses;
using MindLens.Api.Models;
using MindLens.Api.DTOs.Journaling;
using MindLens.Api.Filters;
using MindLens.Api.Models.Enums;

namespace MindLens.Api.Services;

public class JournalingService : IJournalingService
{
    private readonly TenantContext _tenantContext;
    private readonly ApplicationContext _sharedContext;
    private readonly IAwsHelper _awsHelper;
    private readonly ITenantService _tenantService;

    public JournalingService(TenantContext tenantContext, ApplicationContext sharedContext, IAwsHelper awsHelper, ITenantService tenantService)
    {
        _tenantContext = tenantContext;
        _sharedContext = sharedContext;
        _awsHelper = awsHelper;
        _tenantService = tenantService;
    }
    
    public async Task<ServiceResponse<ICollection<Journaling>>> Get(JournalingFilters filters)
    {
        ServiceResponse<ICollection<Journaling>> response = new ServiceResponse<ICollection<Journaling>>();

        // Creating Basic Query
        var query = _tenantContext.Journalings.AsQueryable();
        
        // Applying filters
        if (filters.Date is not null) query = query.Where(j => j.Date == filters.Date);
        if (filters.EntryType is not null) query = query.Where(j => j.EntryType == filters.EntryType);
        if (filters.State is not null) query = query.Where(j => j.State == filters.State);

        // Making query
        query = query.Skip((filters.Page - 1) * filters.PageSize).Take(filters.PageSize);
        
        var journalings = await query.ToListAsync();

        // Returning response
        response.StatusCode = 200;
        response.Message = "Journalings found";
        response.Success = true;
        response.Data = journalings;
        
        return response;
    }
    public async Task<ServiceResponse<ICollection<JournalingAnswer>>> GetAnswers(JournalingAnswerFilters filters)
    {
        ServiceResponse<ICollection<JournalingAnswer>> response = new ServiceResponse<ICollection<JournalingAnswer>>();

        // Creating Basic Query
        var query = _tenantContext.JournalingAnswers.AsQueryable();
        
        // Applying filters
        if (filters.EntryType is not null) query = query.Where(j => j.EntryType == filters.EntryType);
        if (filters.State is not null) query = query.Where(j => j.State == filters.State);

        // Making query
        query = query.Skip((filters.Page - 1) * filters.PageSize).Take(filters.PageSize);

        var journalingAnswers = await query.ToListAsync();
        
        // Returning response
        response.StatusCode = 200;
        response.Message = "Journalings Answers found";
        response.Success = true;
        response.Data = journalingAnswers;
        
        return response;
    }
    public async Task<ServiceResponse<ICollection<JournalingRegisterResponseDto>>> GetRegisters(
        JournalingRegisterFilters filters)
    {
        ServiceResponse<ICollection<JournalingRegisterResponseDto>> response = new ServiceResponse<ICollection<JournalingRegisterResponseDto>>();

        // Creating Basic Query
        var query = _tenantContext.JournalingRegisters.AsQueryable();
        
        // Applying filters
        if (filters.Type is not null) query = query.Where(j => j.Type == filters.Type);

        // Making query
        query = query.Skip((filters.Page - 1) * filters.PageSize).Take(filters.PageSize);

        var journalingRegisters = await query.Select(j => new JournalingRegisterResponseDto() 
        {
            Id = j.Id,
            JournalingId = j.JournalingId,
            AnalyzedContent = j.AnalyzedContent,
            Type = j.Type
        }).ToListAsync();
        
        // Returning response
        response.StatusCode = 200;
        response.Message = "Journalings Registers found";
        response.Success = true;
        response.Data = journalingRegisters;
        
        return response;
    }

    public async Task<ServiceResponse<Journaling>> GetById(Guid id)
    {
        ServiceResponse<Journaling> response = new ServiceResponse<Journaling>();
        
        // Searching
        var journaling = await _tenantContext.Journalings.FindAsync(id);
        
        // Checking it exists
        if (journaling is null)
        {
            response.StatusCode = 404;
            response.Message = "Journaling Not Found";
            response.Success = false;
            
            return response;
        }
        
        // Returning response
        response.StatusCode = 200;
        response.Message = "Journaling Found!";
        response.Success = true;
        response.Data = journaling;
        
        return response;
    }
    public async Task<ServiceResponse<JournalingAnswer>> GetAnswerById(Guid answerId)
    {
        ServiceResponse<JournalingAnswer> response = new ServiceResponse<JournalingAnswer>();
        
        // Searching
        var journalingAnswer = await _tenantContext.JournalingAnswers.FindAsync(answerId);
        
        // Checking it exists
        if (journalingAnswer is null)
        {
            response.StatusCode = 404;
            response.Message = "Journaling Answer Not Found";
            response.Success = false;
            
            return response;
        }
        
        // Returning response
        response.StatusCode = 200;
        response.Message = "Journaling Found!";
        response.Success = true;
        response.Data = journalingAnswer;
        
        return response;
    }
    public async Task<ServiceResponse<JournalingRegisterResponseDto>> GetRegisterById(Guid registerId) 
    {
        ServiceResponse<JournalingRegisterResponseDto> response = new ServiceResponse<JournalingRegisterResponseDto>();
        
        // Searching
        var journalingRegister = await _tenantContext.JournalingRegisters
            .Where(j => j.Id == registerId)
            .Select(j => new JournalingRegisterResponseDto() 
            {
                Id = j.Id,
                JournalingId = j.JournalingId,
                AnalyzedContent = j.AnalyzedContent,
                Type = j.Type
            }).FirstOrDefaultAsync();
        
        // Checking it exists
        if (journalingRegister is null)
        {
            response.StatusCode = 404;
            response.Message = "Journaling Register Not Found";
            response.Success = false;
            
            return response;
        }
        
        // Returning response
        response.StatusCode = 200;
        response.Message = "Journaling Found!";
        response.Success = true;
        response.Data = journalingRegister;
        
        return response;
    }

    public async Task<ServiceResponse> Create(CreateJournalingDto request)
    {
        ServiceResponse response = new ServiceResponse();
        
        // Checking if treatment has any journaling in day
        var journaling = await _tenantContext.Journalings.AnyAsync(j =>
            j.TreatmentId == request.TreatmentId && j.Date == DateOnly.FromDateTime(DateTime.Today));
        
        if(journaling)
        {
            response.StatusCode = 409;
            response.Message = "This treatment has already one journaling registered today";
            response.Success = false;
            
            return response;
        }
        
        // Creating Journaling
        var newJournaling = new Journaling 
        {
            TreatmentId = request.TreatmentId,
            EntryType = request.EntryType,
            IdempotencyKey = request.IdempotencyKey,
            Date = DateOnly.FromDateTime(DateTime.Today) 
        };

        await _tenantContext.Journalings.AddAsync(newJournaling);
        
        // Saving in DB
        await _tenantContext.SaveChangesAsync();
        
        // Publish Event in Amazon Queue
        await _awsHelper.SendProcessingMessageAsync(newJournaling.Id, request.EntryType.ToString(), request.S3Key);
        
        // Returning response
        response.StatusCode = 201;
        response.Message = "Journaling created with success";
        response.Success = true;
        
        return response;
    }
    public async Task<ServiceResponse> CreateAnswer(CreateJournalingAnswerDto request)
    {
        ServiceResponse response = new ServiceResponse();
        
        // Checking if treatment has any journaling answer to same question in day
        var journalingAnswer = await _tenantContext.JournalingAnswers.AnyAsync(j =>
            j.JournalingId == request.JournalingId && j.QuestionId == request.QuestionId);
        
        if(journalingAnswer)
        {
            response.StatusCode = 409;
            response.Message = "This treatment has already one answer registered for that question today";
            response.Success = false;
            
            return response;
        }
        
        // Creating Journaling Answer
        var newJournalingAnswer = new JournalingAnswer 
        {
          JournalingId = request.JournalingId,
          QuestionId = request.QuestionId,
          IdempotencyKey = request.IdempotencyKey,
          EntryType = request.EntryType
        };

        await _tenantContext.JournalingAnswers.AddAsync(newJournalingAnswer);
        
        // Saving in DB
        await _tenantContext.SaveChangesAsync();
        
        // Publish Event in Amazon Queue
        await _awsHelper.SendProcessingMessageAsync(request.JournalingId, request.EntryType.ToString(), request.S3Key);
        
        // Returning response
        response.StatusCode = 201;
        response.Message = "Journaling Answer created with success";
        response.Success = true;
        
        return response;
    }
    
    // Audios Management
    public async Task<ServiceResponse<JournalingUploadAudioResponse>> GetS3Key(Guid userId)
    {
        ServiceResponse<JournalingUploadAudioResponse> response = new ServiceResponse<JournalingUploadAudioResponse>();
        
        // Get Patient Id
        Guid patientId = await _tenantContext.Patients
            .Where(p => p.GlobalUserId == userId)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();
        
        // Checking patient exists
        if (patientId == Guid.Empty)
        {
            response.StatusCode = 404;
            response.Message = "Patient Not Found";
            response.Success = false;
            
            return response;
        }
        
        // Get Tenant Id
        Guid tenantId = _tenantService.CurrentTenant!.Id;

        // Get Treatment Id
        Guid treatmentId = await _tenantContext.Treatments
            .Where(t => t.PatientId == patientId && t.State == TreatmentState.InProcess)
            .Select(t => t.Id)
            .FirstOrDefaultAsync();
        
        // Checking treatment exists
        if (treatmentId == Guid.Empty)
        {
            response.StatusCode = 404;
            response.Message = "Treatment Not Found";
            response.Success = false;
            
            return response;
        }
        
        // Generate S3 Url
        var awsHelperResponse = _awsHelper.GenerateUploadPresignedUrl(tenantId, patientId, treatmentId);

        // Return response
        response.StatusCode = 200;
        response.Success = true;
        response.Message = "URL HTTP Put Successfully Generated";
        response.Data = new JournalingUploadAudioResponse
        {
            UploadUrl = awsHelperResponse.uploadUrl,
            S3Key = awsHelperResponse.s3Key
        };

        return response;
    }
    public async Task<ServiceResponse<JournalingDownloadAudioResponse>> GetDownloadAudio(string s3key)
    {
        ServiceResponse<JournalingDownloadAudioResponse> response = new ServiceResponse<JournalingDownloadAudioResponse>();
        
        // Calling service
        var awsHelperResponse = _awsHelper.GenerateDownloadPresignedUrl(s3key); 
        
        // Returning response
        response.StatusCode = 200;
        response.Success = true;
        response.Message = "Download Audio URL Generated";
        response.Data = new JournalingDownloadAudioResponse
        {
            DownloadUrl = awsHelperResponse
        };

        return response;
    }
}