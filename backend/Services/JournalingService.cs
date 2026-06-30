using Microsoft.EntityFrameworkCore;
using MindLens.Api.Data;
using MindLens.Api.Services.Interfaces;
using MindLens.Api.Responses;
using MindLens.Api.Models;
using MindLens.Api.DTOs.Journaling;
using MindLens.Api.Filters;

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
        
        // Generar URL prefirmada fresca de S3 si aplica
        if (journaling.State == MindLens.Api.Models.Enums.JournalingState.Processed && !string.IsNullOrEmpty(journaling.VoiceRecordKey))
        {
            // Opcional: Generar link para el audio subido (si el cliente necesita escucharlo)
            // No lo reasignamos al modelo para no ensuciarlo, pero si la UI lo espera, se puede hacer
        }

        if (journaling.State == MindLens.Api.Models.Enums.JournalingState.Processed && !string.IsNullOrEmpty(journaling.AiReplyKey))
        {
            // Entregar la URL temporal fresca para que descarguen el audio
            journaling.AiReplyKey = _awsHelper.GenerateDownloadPresignedUrl(journaling.AiReplyKey);
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

    public async Task<ServiceResponse<object>> GetS3Key(Guid patientId, Guid treatmentId)
    {
        ServiceResponse<object> response = new ServiceResponse<object>();
        
        var tenantId = _tenantService.CurrentTenant?.Id ?? Guid.Empty;
        var (uploadUrl, s3Key) = _awsHelper.GenerateUploadPresignedUrl(tenantId, patientId, treatmentId);

        // Return S3 Key
        response.StatusCode = 200;
        response.Message = "S3 Key Sent";
        response.Success = true;
        response.Data = new { uploadUrl, s3Key };

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
        var s3KeyToProcess = request.VoiceRecordKey ?? ""; // Deberia venir en el request si es voz
        await _awsHelper.SendProcessingMessageAsync(newJournaling.Id, newJournaling.EntryType.ToString().ToLower(), s3KeyToProcess);
        
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
        var s3KeyToProcess = request.VoiceRecordKey ?? "";
        await _awsHelper.SendProcessingMessageAsync(newJournalingAnswer.Id, newJournalingAnswer.EntryType.ToString().ToLower(), s3KeyToProcess);
        
        // Returning response
        response.StatusCode = 201;
        response.Message = "Journaling Answer created with success";
        response.Success = true;
        
        return response;
    }
}