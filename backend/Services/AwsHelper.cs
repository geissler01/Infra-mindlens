using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using MindLens.Api.Services.Interfaces;
using System.Text.Json;

namespace MindLens.Api.Services;

public class AwsHelper : IAwsHelper
{
    private readonly IAmazonS3 _s3Client;
    private readonly IAmazonSQS _sqsClient;
    private readonly string _bucketName;
    private readonly string _queueUrl;
    private readonly bool _isLocal;

    public AwsHelper(IAmazonS3 s3Client, IAmazonSQS sqsClient, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _sqsClient = sqsClient;
        _queueUrl = configuration["AWS:QueueUrl"] ?? "";
        _bucketName = configuration["AWS:AudioBucketName"] ?? "";
        _isLocal = !string.IsNullOrEmpty(configuration["AWS:EndpointUrl"]);
    }

    // Generating S3 Upload Audio Url
    public (string uploadUrl, string s3Key) GenerateUploadPresignedUrl(Guid tenantId, Guid patientId, Guid treatmentId)
    {
        var s3Key = $"audios/tenant_{tenantId}/patient_{patientId}/treatment_{treatmentId}/{Guid.NewGuid()}.m4a";
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = s3Key,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(15),
            Protocol = _isLocal ? Protocol.HTTP : Protocol.HTTPS
        };

        var url = _s3Client.GetPreSignedURL(request);
        if (_isLocal && url.Contains("localstack")) 
        {
            url = url.Replace("localstack", "localhost");
        }
        
        return (url, s3Key);
    }

    // Generating Download Audio URL
    public string GenerateDownloadPresignedUrl(string s3Key)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = s3Key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddMinutes(15),
            Protocol = _isLocal ? Protocol.HTTP : Protocol.HTTPS
        };

        var url = _s3Client.GetPreSignedURL(request);
        if (_isLocal && url.Contains("localstack")) 
        {
            url = url.Replace("localstack", "localhost");
        }
        
        return url;
    }

    // Queue Publishing Event
    public async Task SendProcessingMessageAsync(Guid journalingId, string entryType, string s3Key)
    {
        if (string.IsNullOrEmpty(_queueUrl)) return;

        var messageBody = JsonSerializer.Serialize(new { JournalingId = journalingId, EntryType = entryType, S3Key = s3Key });
        var request = new SendMessageRequest
        {
            QueueUrl = _queueUrl,
            MessageBody = messageBody
        };

        await _sqsClient.SendMessageAsync(request);
    }
}