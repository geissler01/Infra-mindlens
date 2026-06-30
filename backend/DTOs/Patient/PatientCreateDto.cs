using System.ComponentModel.DataAnnotations;

namespace MindLens.Api.DTOs.Patient;

// <summary>Create a new Patient</summary>
public class PatientCreateDto
{
    [Required] [StringLength(15, MinimumLength = 3)] public string FirstName { get; set; } = string.Empty;
    [Required] [StringLength(15, MinimumLength = 3)] public string LastName { get; set; } = string.Empty;
    
    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    public string EmergencyPhone { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 5)]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [StringLength(10, MinimumLength = 3)]
    public string AgeRange { get; set; } = string.Empty;
    
    [Required]
    [StringLength(15, MinimumLength = 3)]
    public string RelationshipStatus { get; set; } = string.Empty;
    
    [Required]
    [StringLength(60, MinimumLength = 3)]
    public string Occupation { get; set; } = string.Empty;
    
    [Required]
    [StringLength(60, MinimumLength = 3)]
    public string LivingSituation { get; set; } = string.Empty;
    
    [Required]
    [StringLength(60, MinimumLength = 3)]
    public string PrimaryGoal { get; set; } = string.Empty;
    
    [Required]
    public bool HasPreviousTherapy { get; set; }
}