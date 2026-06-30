namespace MindLens.Api.Services.Interfaces;

public interface IAwsHelper
{
    (string uploadUrl, string s3Key) GenerateUploadPresignedUrl(Guid tenantId, Guid patientId, Guid treatmentId);
    string GenerateDownloadPresignedUrl(string s3Key);
    Task SendProcessingMessageAsync(Guid journalingId, string entryType, string s3Key);
}
