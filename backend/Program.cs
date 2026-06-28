using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configurar base de datos Postgres (Obtener connection string de variable de entorno)
var dbHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
var dbPass = Environment.GetEnvironmentVariable("DB_PASS") ?? "postgres";
var connectionString = $"Host={dbHost};Database=journaldb;Username=postgres;Password={dbPass}";

builder.Services.AddDbContext<JournalContext>(options =>
    options.UseNpgsql(connectionString));

// Configurar AWS SQS
builder.Services.AddAWSService<IAmazonSQS>();

var app = builder.Build();

// Crear la base de datos automáticamente al inicio para simplificar la PoC
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<JournalContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/api/status", () => Results.Ok(new { Status = "Backend is running!" }));

app.MapPost("/journal/entry", async (JournalRequest request, JournalContext db, IAmazonSQS sqs) =>
{
    // 1. Guardar en Base de Datos
    var entry = new JournalEntry
    {
        PacienteId = request.PacienteId,
        Texto = request.Texto,
        Status = "processing"
    };
    db.JournalEntries.Add(entry);
    await db.SaveChangesAsync();

    // 2. Enviar a SQS
    var queueUrl = Environment.GetEnvironmentVariable("SQS_QUEUE_URL");
    if (!string.IsNullOrEmpty(queueUrl))
    {
        var messageBody = JsonSerializer.Serialize(new { EntryId = entry.Id, Texto = entry.Texto });
        await sqs.SendMessageAsync(new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = messageBody
        });
    }

    return Results.Accepted($"/journal/entry/{entry.Id}", new { entryId = entry.Id });
});

app.MapGet("/journal/entry/{id}", async (int id, JournalContext db) =>
{
    var entry = await db.JournalEntries.FindAsync(id);
    if (entry == null) return Results.NotFound();
    
    return Results.Ok(new { 
        id = entry.Id, 
        status = entry.Status, 
        respuesta = entry.Respuesta 
    });
});

app.Run();

// Modelos
public class JournalRequest
{
    public int PacienteId { get; set; }
    public string Texto { get; set; } = string.Empty;
}

public class JournalEntry
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public string Texto { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Respuesta { get; set; }
}

public class JournalContext : DbContext
{
    public JournalContext(DbContextOptions<JournalContext> options) : base(options) { }
    public DbSet<JournalEntry> JournalEntries { get; set; }
}
