using MindLens.Api.Models.Enums;

namespace MindLens.Api.Filters;

public class QuestionFilters : PaginationFilters
{
    public string? Question { get; set; }
    public QuestionType? Type { get; set; }
}