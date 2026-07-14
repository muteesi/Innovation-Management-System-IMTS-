using IMTS.Shared.Dtos;
using IMTS.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<EmailDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost;Database=InnovationManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapPost("/api/email/send", async ([FromBody] SendEmailRequest request, EmailDbContext db = null!) =>
{
    var entity = new EmailQueueItem
    {
        ToAddress = request.ToAddress,
        Subject = request.Subject,
        BodyHtml = request.BodyHtml,
        TemplateKey = request.TemplateKey,
        Status = "Queued",
        CreatedBy = "system"
    };

    db.EmailQueueItems.Add(entity);
    await db.SaveChangesAsync();
    return Results.Accepted($"/api/email/queue/{entity.Id}", new { id = entity.Id, status = "Queued" });
});

app.MapGet("/api/email/queue", async (EmailDbContext db = null!) =>
{
    var items = await db.EmailQueueItems.AsNoTracking().Where(e => !e.Deleted).OrderByDescending(e => e.CreatedDate).ToListAsync();
    return Results.Ok(items.Select(e => new EmailQueueItemDto(e.Id, e.ToAddress, e.Subject, e.Status, e.RetryCount, e.SentAt, e.ErrorMessage)));
});

app.MapPost("/api/email/templates/seed", async (EmailDbContext db = null!) =>
{
    var templates = new[]
    {
        new EmailTemplate { TemplateKey = "idea-submitted", Subject = "Idea Submitted", BodyHtml = "<p>Your innovation idea has been received.</p>", Description = "Idea Submitted", IsActive = true, CreatedDate = DateTimeOffset.UtcNow },
        new EmailTemplate { TemplateKey = "idea-approved", Subject = "Idea Approved", BodyHtml = "<p>Your innovation idea has been approved.</p>", Description = "Idea Approved", IsActive = true, CreatedDate = DateTimeOffset.UtcNow },
        new EmailTemplate { TemplateKey = "resource-uploaded", Subject = "Resource Uploaded", BodyHtml = "<p>A new resource is now available.</p>", Description = "Resource Uploaded", IsActive = true, CreatedDate = DateTimeOffset.UtcNow }
    };

    db.EmailTemplates.AddRange(templates);
    await db.SaveChangesAsync();
    return Results.Ok(new { created = templates.Length });
});

SeedEmailDatabase(app);
app.Run();

static void SeedEmailDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<EmailDbContext>();
    if (db.Database.GetMigrations().Any())
    {
        db.Database.Migrate();
    }
    else
    {
        db.Database.EnsureCreated();
    }

    if (!db.EmailTemplates.Any())
    {
        db.EmailTemplates.AddRange(
            new EmailTemplate { TemplateKey = "idea-submitted", Subject = "Idea Submitted", BodyHtml = "<p>Your innovation idea has been received.</p>", Description = "Idea Submitted", IsActive = true, CreatedDate = DateTimeOffset.UtcNow },
            new EmailTemplate { TemplateKey = "idea-approved", Subject = "Idea Approved", BodyHtml = "<p>Your innovation idea has been approved.</p>", Description = "Idea Approved", IsActive = true, CreatedDate = DateTimeOffset.UtcNow },
            new EmailTemplate { TemplateKey = "resource-uploaded", Subject = "Resource Uploaded", BodyHtml = "<p>A new resource is now available.</p>", Description = "Resource Uploaded", IsActive = true, CreatedDate = DateTimeOffset.UtcNow }
        );
    }

    if (!db.EmailQueueItems.Any())
    {
        db.EmailQueueItems.Add(new EmailQueueItem
        {
            ToAddress = "noreply@bou.or.ug",
            Subject = "Welcome to IMTS",
            BodyHtml = "<p>Welcome to the IMTS innovation portal. Your email notifications are now enabled.</p>",
            Status = "Queued",
            CreatedDate = DateTimeOffset.UtcNow
        });
    }

    db.SaveChanges();
}

public class EmailDbContext : DbContext
{
    public EmailDbContext(DbContextOptions<EmailDbContext> options) : base(options) { }

    public DbSet<EmailQueueItem> EmailQueueItems => Set<EmailQueueItem>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailQueueItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ToAddress).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Subject).HasMaxLength(200).IsRequired();
            entity.Property(e => e.TemplateKey).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<EmailTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TemplateKey).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Subject).HasMaxLength(200).IsRequired();
        });
    }
}

