using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Text.Json;

namespace Backend.Services
{
    public class AwsHelper
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IAmazonSQS _sqsClient;
        private readonly string _bucketName = "journal-audios-bucket";
        private readonly string _queueUrl;
        private readonly bool _isLocal;

        public AwsHelper(IAmazonS3 s3Client, IAmazonSQS sqsClient)
        {
            _s3Client = s3Client;
            _sqsClient = sqsClient;
            _queueUrl = Environment.GetEnvironmentVariable("SQS_QUEUE_URL") ?? "";
            _isLocal = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_ENDPOINT_URL"));
        }

        public string GenerateUploadPresignedUrl(string patientId)
        {
            var s3Key = $"audios/{patientId}/{Guid.NewGuid()}.m4a";
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
            
            return JsonSerializer.Serialize(new { uploadUrl = url, s3Key = s3Key });
        }

        public string GenerateDownloadPresignedUrl(string s3Key)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucketName,
                Key = s3Key,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(60),
                Protocol = _isLocal ? Protocol.HTTP : Protocol.HTTPS
            };

            var url = _s3Client.GetPreSignedURL(request);
            if (_isLocal && url.Contains("localstack")) 
            {
                url = url.Replace("localstack", "localhost");
            }
            
            return url;
        }

        public async Task SendProcessingMessageAsync(int journalingId, string s3Key)
        {
            if (string.IsNullOrEmpty(_queueUrl)) return;

            var messageBody = JsonSerializer.Serialize(new { EntryId = journalingId, S3Key = s3Key });
            var request = new SendMessageRequest
            {
                QueueUrl = _queueUrl,
                MessageBody = messageBody
            };

            await _sqsClient.SendMessageAsync(request);
        }
    }
}
