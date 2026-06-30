namespace MindLens.Api.DTOs.Patient;

public class PatientGetByIdResponse
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string EmergencyPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string AgeRange { get; set; } = string.Empty;
    public string RelationshipStatus { get; set; } = string.Empty;
    public string Occupation { get; set; } = string.Empty;
    public string LivingSituation { get; set; } = string.Empty;
    public string PrimaryGoal { get; set; } = string.Empty;
    public bool HasPreviousTherapy { get; set; }
    
    public DateTime CreatedAt { get; set; }
}