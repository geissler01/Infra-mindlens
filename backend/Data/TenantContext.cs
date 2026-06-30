using Microsoft.EntityFrameworkCore;
using MindLens.Api.Models;
using MindLens.Api.Models.Enums;

namespace MindLens.Api.Data;

public class TenantContext : DbContext
{
    public TenantContext(DbContextOptions<TenantContext> options) : base(options) {}
    
    // Models
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Treatment> Treatments { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<TreatmentQuestion> TreatmentQuestions { get; set; }
    public DbSet<Journaling> Journalings { get; set; }
    public DbSet<JournalingRegister> JournalingRegisters { get; set; }
    public DbSet<JournalingAnswer> JournalingAnswers { get; set; }
    public DbSet<WeeklyReport> WeeklyReports { get; set; }
    public DbSet<WeeklyClusterReport> WeeklyClusterReports { get; set; }
    public DbSet<PresessionReport> PresessionReports { get; set; }
    
    // Model Configuration
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Postgres Extension
        builder.HasPostgresExtension("vector");

        // Patient
        builder.Entity<Patient>(entity =>
        {
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(x => x.HasPreviousTherapy).HasDefaultValue(false);
        });

        // Treatment
        builder.Entity<Treatment>(entity =>
        {
            entity.Property(x => x.StartedAt).HasDefaultValueSql("CURRENT_DATE");
            entity.Property(x => x.SessionDay)
                .HasConversion<string>();
            entity.Property(x => x.State)
                .HasConversion<string>()
                .HasDefaultValue(TreatmentState.InProcess);

            entity.HasOne(x => x.Patient)
                .WithMany(p => p.Treatments)
                .HasForeignKey(x => x.PatientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Notes
        builder.Entity<Note>(entity =>
        {
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(x => x.Treatment)
                .WithMany(t => t.Notes)
                .HasForeignKey(x => x.TreatmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Questions
        builder.Entity<Question>(entity =>
        {
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(x => x.Type).HasConversion<string>().HasColumnName("pillar_type");
        });

        // Treatment Questions
        builder.Entity<TreatmentQuestion>(entity =>
        {
            entity.HasOne(x => x.Treatment)
                .WithMany(t => t.TreatmentQuestions)
                .HasForeignKey(x => x.TreatmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Question)
                .WithMany(q => q.TreatmentQuestions)
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Journalings
        builder.Entity<Journaling>(entity =>
        {
            entity.Property(x => x.EntryType)
                .HasConversion<string>()
                .HasDefaultValue(JournalingEntryType.Text);
            entity.Property(x => x.State)
                .HasConversion<string>()
                .HasDefaultValue(JournalingState.Pending);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
            
            entity.HasOne(x => x.Treatment)
                .WithMany(t => t.Journalings)
                .HasForeignKey(x => x.TreatmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Journaling Registers
        builder.Entity<JournalingRegister>(entity =>
        {
            entity.Property(x => x.Type).HasConversion<string>().HasColumnName("pillar_type");
            entity.Property(x => x.Embedding).HasColumnType("vector(1536)");

            entity.HasOne(x => x.Journaling)
                .WithMany(j => j.JournalingRegisters)
                .HasForeignKey(x => x.JournalingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.Embedding)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");
        });

        // Journaling Answers
        builder.Entity<JournalingAnswer>(entity =>
        {
            entity.Property(x => x.EntryType)
                .HasConversion<string>()
                .HasDefaultValue(JournalingEntryType.Text);
            entity.Property(x => x.State)
                .HasConversion<string>()
                .HasDefaultValue(JournalingState.Pending);
            
            entity.HasOne(x => x.Journaling)
                .WithMany(j => j.JournalingAnswers)
                .HasForeignKey(x => x.JournalingId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Question)
                .WithMany(q => q.JournalingAnswers)
                .HasForeignKey(x => x.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Weekly Reports
        builder.Entity<WeeklyReport>(entity =>
        {
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(x => x.Treatment)
                .WithMany(t => t.WeeklyReports)
                .HasForeignKey(x => x.TreatmentId)
                .OnDelete(DeleteBehavior.Restrict);
});
        
        // Weekly Cluster Reports
        builder.Entity<WeeklyClusterReport>(entity =>
        {
            entity.Property(x => x.Type).HasConversion<string>().HasColumnName("pillar_type");
            entity.Property(x => x.ClusterEmbedding).HasColumnType("vector(1536)");

            entity.HasOne(x => x.WeeklyReport)
                .WithMany(w => w.WeeklyClusterReports)
                .HasForeignKey(x => x.WeeklyReportId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(x => x.ClusterEmbedding)
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");;
        });
        
        // Pre Session Reports
        builder.Entity<PresessionReport>(entity =>
        {
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("NOW()");
            
            entity.HasOne(x => x.Treatment)
                .WithOne(p => p.PresessionReport)
                .HasForeignKey<PresessionReport>()
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}