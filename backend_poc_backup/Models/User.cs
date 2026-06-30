using Microsoft.AspNetCore.Identity;
using MindLens.Api.Models.Enums;

namespace MindLens.Api.Models;

public class User : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public PsychologistProfile? Profile { get; set; }
    public Tenant? Tenant { get; set; }
}