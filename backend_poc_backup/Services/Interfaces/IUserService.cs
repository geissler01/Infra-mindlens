using MindLens.Api.DTOs.User;
using MindLens.Api.Filters;
using MindLens.Api.Models;
using MindLens.Api.Responses;

namespace MindLens.Api.Services.Interfaces;

public interface IUserService
{
    public Task<ServiceResponse> Store(AddUserDto request);
    public Task<ServiceResponse<ICollection<User>>> Get(UserFilters filters);
    public Task<ServiceResponse<User>> GetById(Guid id);
    public Task<ServiceResponse> Update(Guid id, UpdateUserDto request);
}