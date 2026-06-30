using MindLens.Api.Models.Enums;

namespace MindLens.Api.Models;

public class JournalingAnswer
{
    public Guid Id { get; set;  }

    public JournalingEntryType EntryType { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? VoiceRecordUrl { get; set; }
    public string? Transcription { get; set; }
    public JournalingState State { get; set; }
    public Guid JournalingId { get; set; }
    public Guid QuestionId { get; set; }

    public Journaling Journaling { get; set; } = new Journaling();
    public Question Question { get; set; } = new Question();
}