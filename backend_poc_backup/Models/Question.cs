using MindLens.Api.Models.Enums;

namespace MindLens.Api.Models;

public class Question
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public List<TreatmentQuestion> TreatmentQuestions { get; set; } = new List<TreatmentQuestion>();
    public List<JournalingAnswer> JournalingAnswers { get; set; } = new List<JournalingAnswer>();
}