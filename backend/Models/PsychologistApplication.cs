using MindLens.Api.Models.Enums;

namespace MindLens.Api.Models;

public class PsychologistApplication
{
    public Guid Id { get; set;  }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public PsychologistApplicationState State { get; set; }
    public DateTime CreatedAt { get; set; }
}