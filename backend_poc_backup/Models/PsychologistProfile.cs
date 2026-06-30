namespace MindLens.Api.Models;

public class PsychologistProfile
{
    public Guid Id { get; set; }
    public string Speciality { get; set; } = string.Empty;
    public string ProfilePhotoUrl { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public string Biography { get; set; } = string.Empty;
    public Guid PsychologistId { get; set; }

    public User Psychologist { get; set; } = new User();
}