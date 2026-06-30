namespace MindLens.Api.Models;

public class WeeklyReport
{
    public Guid Id { get; set; }
    public string Summary { get; set; } = string.Empty;
    public DateOnly StartedAt { get; set; }
    public DateOnly FinishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid TreatmentId { get; set; }

    public Treatment Treatment { get; set; } = new Treatment();

    public List<WeeklyClusterReport> WeeklyClusterReports { get; set; } = new List<WeeklyClusterReport>();
}