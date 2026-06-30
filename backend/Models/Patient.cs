namespace MindLens.Api.Models;

public class Patient
{
    public Guid Id { get; set; }
    public Guid GlobalUserId { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string EmergencyPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string AgeRange { get; set; } = string.Empty;
    public string RelationshipStatus { get; set; }
    public string Occupation { get; set; } = string.Empty;
    public string LivingSituation { get; set; }
    public string PrimaryGoal { get; set; }
    public bool HasPreviousTherapy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<Treatment> Treatments { get; set; } = new List<Treatment>();
}