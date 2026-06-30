using MindLens.Api.DTOs.Auth;
using MindLens.Api.Responses;

namespace MindLens.Api.Services.Interfaces;

public interface IAuthService
{
    public Task<ServiceResponse<string>> Login(LoginRequestDto request);
    public Task<ServiceResponse<string>> Register(RegisterRequestDto request);
}