using MindLens.Api.Models.Enums;

namespace MindLens.Api.Models;

public class Journaling
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public JournalingEntryType EntryType { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? Transcription { get; set; }
    public string? VoiceRecordKey { get; set; }
    public string? AiReplyKey { get; set; }
    public string? AiReplyText { get; set; }
    public JournalingState State { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid TreatmentId { get; set; }

    public Treatment Treatment { get; set; } = new Treatment();

    public List<JournalingRegister> JournalingRegisters { get; set; } = new List<JournalingRegister>();
    public List<JournalingAnswer> JournalingAnswers { get; set; } = new List<JournalingAnswer>();
}