using Backend.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

var builder = WebApplication.CreateBuilder(args);

// Configurar base de datos Postgres
var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbName = Environment.GetEnvironmentVariable("DB_NAME") ?? "journal_db";
var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "journal_user";
var dbPass = Environment.GetEnvironmentVariable("DB_PASS") ?? "journal_pass";
var connectionString = $"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPass}";

builder.Services.AddDbContext<JournalContext>(options => options.UseNpgsql(connectionString));

// Configurar AWS SQS y S3
var awsEndpoint = Environment.GetEnvironmentVariable("AWS_ENDPOINT_URL");
if (!string.IsNullOrEmpty(awsEndpoint))
{
    var sqsConfig = new Amazon.SQS.AmazonSQSConfig { ServiceURL = awsEndpoint };
    builder.Services.AddSingleton<Amazon.SQS.IAmazonSQS>(new Amazon.SQS.AmazonSQSClient(sqsConfig));
    
    var s3Config = new Amazon.S3.AmazonS3Config { ServiceURL = awsEndpoint, ForcePathStyle = true };
    builder.Services.AddSingleton<Amazon.S3.IAmazonS3>(new Amazon.S3.AmazonS3Client(s3Config));
}
else
{
    builder.Services.AddAWSService<Amazon.SQS.IAmazonSQS>();
    builder.Services.AddAWSService<Amazon.S3.IAmazonS3>();
}

builder.Services.AddSingleton<AwsHelper>();

var app = builder.Build();

app.MapGet("/api/status", () => Results.Ok(new { Status = "Backend is running!" }));

// 1. Obtener URL para subir el audio
app.MapGet("/api/journal/upload-url", (AwsHelper awsHelper) =>
{
    var result = awsHelper.GenerateUploadPresignedUrl("treatment_1");
    return Results.Content(result, "application/json");
});

// 2. Avisar al Backend que el audio ya se subió y encolar tarea
app.MapPost("/api/journal/entry", async (JournalRequest request, JournalContext db, AwsHelper awsHelper) =>
{
    var entry = new Journaling
    {
        TreatmentId = request.TreatmentId,
        Date = DateOnly.FromDateTime(DateTime.UtcNow),
        VoiceRecordKey = request.S3Key,
        Status = "processing"
    };
    
    db.Journalings.Add(entry);
    await db.SaveChangesAsync();

    // Enviar a SQS
    await awsHelper.SendProcessingMessageAsync(entry.Id, request.S3Key);

    return Results.Accepted($"/api/journal/entry/{entry.Id}", new { entryId = entry.Id });
});

// 3. Polling para ver si la IA terminó
app.MapGet("/api/journal/entry/{id}", async (int id, JournalContext db, AwsHelper awsHelper) =>
{
    var entry = await db.Journalings.FindAsync(id);
    if (entry == null) return Results.NotFound();
    
    if (entry.Status == "completed")
    {
        // Simulación de correo en caso de emergencia
        if (entry.IsEmergency == true)
        {
            Console.WriteLine($"[ALERTA DE EMERGENCIA] ⚠️ Enviando SMS/Correo al psicólogo del tratamiento {entry.TreatmentId} por el Journal {entry.Id}");
        }

        var downloadUrl = string.IsNullOrEmpty(entry.AiReplyKey) 
            ? null 
            : awsHelper.GenerateDownloadPresignedUrl(entry.AiReplyKey);

        return Results.Ok(new { 
            id = entry.Id, 
            status = entry.Status, 
            ai_reply_url = downloadUrl,
            ai_reply_text = entry.AiReplyText,
            is_emergency = entry.IsEmergency
        });
    }

    return Results.Ok(new { id = entry.Id, status = entry.Status });
});

app.Run();

// --- Modelos ---
public class JournalRequest
{
    public int TreatmentId { get; set; }
    public string S3Key { get; set; } = string.Empty;
}

[Table("journalings")]
public class Journaling
{
    [Column("id")]
    public int Id { get; set; }
    
    [Column("treatment_id")]
    public int TreatmentId { get; set; }
    
    [Column("date")]
    public DateOnly Date { get; set; }
    
    [Column("status")]
    public string Status { get; set; } = "processing";
    
    [Column("voice_record_key")]
    public string? VoiceRecordKey { get; set; }
    
    [Column("ai_reply_key")]
    public string? AiReplyKey { get; set; }

    [Column("ai_reply_text")]
    public string? AiReplyText { get; set; }
    
    [Column("is_emergency")]
    public bool? IsEmergency { get; set; }
    
    [Column("transcription")]
    public string? Transcription { get; set; }
}

public class JournalContext : DbContext
{
    public JournalContext(DbContextOptions<JournalContext> options) : base(options) { }
    public DbSet<Journaling> Journalings { get; set; }
}
