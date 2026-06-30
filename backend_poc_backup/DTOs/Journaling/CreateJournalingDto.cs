using System.ComponentModel.DataAnnotations;
using MindLens.Api.Models.Enums;

namespace MindLens.Api.DTOs.Journaling;

public class CreateJournalingDto
{
    [Required]
    public Guid TreatmentId { get; set; }
    
    [Required]
    [EnumDataType(typeof(JournalingEntryType))]
    public JournalingEntryType EntryType { get; set; }

    public string? IdempotencyKey { get; set; } = string.Empty;
}