namespace MindLens.Api.Models;

public class PresessionReport
{
    public Guid Id { get; set; }
    public string FlashBriefing { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid TreatmentId { get; set; }

    public Treatment Treatment { get; set; } = new Treatment();
}