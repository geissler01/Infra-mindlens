using MindLens.Api.Models.Enums;

namespace MindLens.Api.DTOs.Journaling;

public class JournalingRegisterResponseDto
{
    public Guid Id { get; set; }
    public Guid JournalingId { get; set; }
    public JournalingRegisterType Type { get; set; }
    public string AnalyzedContent { get; set; } = string.Empty;
}