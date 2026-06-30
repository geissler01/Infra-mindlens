using MindLens.Api.Models.Enums;

namespace MindLens.Api.Filters;

public class UserFilters : PaginationFilters
{
    public UserRole? Role { get; init; }
}