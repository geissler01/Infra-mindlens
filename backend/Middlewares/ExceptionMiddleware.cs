namespace MindLens.Api.Middlewares;

using MindLens.Api.Responses;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task Invoke(HttpContext context) 
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            ServiceResponse response = new ServiceResponse
            {
                StatusCode = 500,
                Message = "Internal Server Error",
                Errors = new List<string>
                {
                    $"Internal Message: {ex.Message}",
                    $"Stack Trace: {ex.StackTrace}",
                    $"Inner Exception: {ex.InnerException?.Message}"
                }
            };
            
            context.Response.StatusCode = response.StatusCode;
            
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}