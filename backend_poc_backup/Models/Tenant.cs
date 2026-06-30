using MindLens.Api.Models.Enums;

namespace MindLens.Api.Models;

public class Tenant
{
    public Guid Id { get; set; }
    public string? Domain { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public TenantState State { get; set; }
    public Guid PsychologistId { get; set; }

    public User Psychologist { get; set; } = new User();
}