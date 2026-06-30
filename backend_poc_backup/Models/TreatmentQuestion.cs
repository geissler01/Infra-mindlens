namespace MindLens.Api.Models;

public class TreatmentQuestion
{
    public Guid Id { get; set; }
    public Guid TreatmentId { get; set; }
    public Guid QuestionId { get; set; }

    public Treatment Treatment { get; set; } = new Treatment();
    public Question Question { get; set; } = new Question();
}