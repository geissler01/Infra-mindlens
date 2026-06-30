using MindLens.Api.Models.Enums;
using Pgvector;

namespace MindLens.Api.Models;

public class WeeklyClusterReport
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public WeeklyClusterReportType Type { get; set; }
    public int Repetitions { get; set; }
    public Vector ClusterEmbedding { get; set; } = null!;
    public Guid WeeklyReportId { get; set; }

    public WeeklyReport WeeklyReport { get; set; } = new WeeklyReport();
}