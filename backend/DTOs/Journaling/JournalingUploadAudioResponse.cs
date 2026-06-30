namespace MindLens.Api.DTOs.Journaling;

public class JournalingUploadAudioResponse
{
    public string UploadUrl { get; set; } =  string.Empty;
    public string S3Key { get; set; } = string.Empty;
}