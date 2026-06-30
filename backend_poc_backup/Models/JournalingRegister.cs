using MindLens.Api.Models.Enums;
using Pgvector;

namespace MindLens.Api.Models;

public class JournalingRegister
{
    public Guid Id { get; set; }
    public JournalingRegisterType Type { get; set; }
    public string AnalyzedContent { get; set; } = string.Empty;
    public Vector Embedding { get; set; } = null!;
    public Guid JournalingId { get; set; }

    public Journaling Journaling { get; set; } = new Journaling();
}