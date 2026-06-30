using MindLens.Api.Models.Enums;

namespace MindLens.Api.Models;

public class Treatment
{
    public Guid Id { get; set; }
    public DateOnly StartedAt { get; set; }
    public DateOnly? FinishedAt { get; set; }
    public TreatmentSessionDay SessionDay { get; set; }
    public TreatmentState State { get; set; }
    public Guid PatientId { get; set; }
    
    public Patient Patient { get; set; } = new Patient();
    public PresessionReport PresessionReport { get; set; } = new PresessionReport();
    
    public List<Note> Notes { get; set; } = new List<Note>();
    public List<Journaling> Journalings { get; set; } = new List<Journaling>();
    public List<TreatmentQuestion> TreatmentQuestions { get; set; } = new List<TreatmentQuestion>();
    public List<WeeklyReport> WeeklyReports { get; set; } = new List<WeeklyReport>();
}