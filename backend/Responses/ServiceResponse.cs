namespace MindLens.Api.Responses;

public class ServiceResponse
{
    public string? Message { get; set; }
    public bool Success { get; set;  }
    public int StatusCode { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}

public class ServiceResponse<T> : ServiceResponse
{
    public T? Data { get; set; }
} 