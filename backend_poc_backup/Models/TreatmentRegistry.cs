using MindLens.Api.Models.Enums;

namespace MindLens.Api.Models;

public class TreatmentRegistry
{
    public Guid Id { get; set; }
    public Guid PsychologisstId { get; set; }
    public Guid PatientId { get; set; }
    public TreatmentRegistryState State { get; set; }
}