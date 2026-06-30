using MindLens.Api.Models;

namespace MindLens.Api.Services.Interfaces;

public interface IJwtService
{
    public Task<string> GenerateTokenAsync(User user);
}