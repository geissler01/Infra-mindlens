using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MindLens.Api.Models;
using MindLens.Api.Models.Enums;

namespace MindLens.Api.Data;

public class ApplicationContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
    {}

    // Models
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<PsychologistProfile> Profiles { get; set; }
    public DbSet<PsychologistApplication> PsychologistApplications { get; set; }
    public DbSet<TreatmentRegistry> TreatmentsRegistry { get; set; }
    
    // Models Configuration
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // User
        builder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>()
            .HasDefaultValue(UserRole.Patient);
        builder.Entity<User>()
            .Property(u => u.CreatedAt)
            .HasDefaultValueSql("NOW()");

        // Tenant
        builder.Entity<Tenant>()
            .Property(t => t.State)
            .HasConversion<string>()
            .HasDefaultValue(TenantState.Pending);
        builder.Entity<Tenant>()
            .HasOne(t => t.Psychologist)
            .WithOne(u => u.Tenant)
            .HasForeignKey<Tenant>(t => t.PsychologistId)
            .OnDelete(DeleteBehavior.Restrict);

        // Psychologist Profile
        builder.Entity<PsychologistProfile>()
            .HasOne(p => p.Psychologist)
            .WithOne(u => u.Profile)
            .HasForeignKey<PsychologistProfile>(p => p.PsychologistId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Treatment Registry
        builder.Entity<TreatmentRegistry>()
            .Property(t => t.State)
            .HasConversion<string>()
            .HasDefaultValue(TreatmentRegistryState.InProcess);
        
        // Psychologist Application
        builder.Entity<PsychologistApplication>()
            .Property(p => p.CreatedAt)
            .HasDefaultValueSql("NOW()");
        builder.Entity<PsychologistApplication>()
            .Property(p => p.State)
            .HasConversion<string>()
            .HasDefaultValue(PsychologistApplicationState.Pending);
    }
}