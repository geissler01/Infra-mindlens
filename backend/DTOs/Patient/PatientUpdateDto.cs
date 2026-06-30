using System.ComponentModel.DataAnnotations;

namespace MindLens.Api.DTOs.Patient;

public class PatientUpdateDto
{
    [Phone]
    public string? Phone { get; set; }
    
    [Phone]
    public string? EmergencyPhone { get; set; }
    
    [StringLength(100, MinimumLength = 5)]
    public string? Address { get; set; }
    
    [StringLength(10, MinimumLength = 3)]
    public string? AgeRange { get; set; }
    
    [StringLength(15, MinimumLength = 3)]
    public string? RelationshipStatus { get; set; }
    
    [StringLength(60, MinimumLength = 3)]
    public string? Occupation { get; set; }
    
    [StringLength(60, MinimumLength = 3)]
    public string? LivingSituation { get; set; }
    
    [StringLength(60, MinimumLength = 3)]
    public string? PrimaryGoal { get; set; }
}