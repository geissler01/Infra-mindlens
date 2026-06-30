using System.ComponentModel.DataAnnotations;
using MindLens.Api.Models.Enums;

namespace MindLens.Api.DTOs.Question;

public class QuestionCreateDto
{
    [Required]
    [StringLength(100, MinimumLength = 5)]
    public string Question { get; set; } = string.Empty;
    
    [Required]
    [EnumDataType(typeof(QuestionType))]
    public QuestionType Type { get; set; }
}