using IMTS.Shared.Dtos;
using IMTS.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost;Database=InnovationManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapGet("/api/notifications", async ([FromQuery] string? userId, [FromQuery] string? filter, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, NotificationDbContext db = null!) =>
{
    IQueryable<Notification> query = db.Notifications.AsNoTracking().Where(n => !n.Deleted).OrderByDescending(n => n.CreatedDate);

    if (!string.IsNullOrWhiteSpace(userId))
        query = query.Where(n => n.RecipientUserId == userId);

    if (filter == "unread")
        query = query.Where(n => !n.IsRead);
    else if (filter == "read")
        query = query.Where(n => n.IsRead);

    var total = await query.CountAsync();
    var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

    return Results.Ok(new
    {
        items = items.Select(n => new
        {
            id = n.Id,
            title = n.Title,
            message = n.Message,
            type = n.Type,
            date = n.CreatedDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm"),
            read = n.IsRead,
            actionUrl = n.ActionUrl
        }),
        totalCount = total,
        page,
        pageSize
    });
});

app.MapGet("/api/notifications/unread-count", async ([FromQuery] string? userId, NotificationDbContext db = null!) =>
{
    var query = db.Notifications.AsNoTracking().Where(n => !n.Deleted && !n.IsRead);
    if (!string.IsNullOrWhiteSpace(userId)) query = query.Where(n => n.RecipientUserId == userId);
    var count = await query.CountAsync();
    return Results.Ok(new { unreadCount = count });
});

app.MapPost("/api/notifications", async ([FromBody] NotificationCreateRequest request, NotificationDbContext db = null!) =>
{
    var entity = new Notification
    {
        Title = request.Title,
        Message = request.Message,
        Type = request.Type,
        RecipientUserId = request.RecipientUserId,
        ActionUrl = request.ActionUrl,
        CreatedBy = "system"
    };

    db.Notifications.Add(entity);
    await db.SaveChangesAsync();
    return Results.Created($"/api/notifications/{entity.Id}", new { id = entity.Id });
});

app.MapPut("/api/notifications/{id}/read", async (int id, NotificationDbContext db = null!) =>
{
    var entity = await db.Notifications.FirstOrDefaultAsync(n => n.Id == id && !n.Deleted);
    if (entity is null) return Results.NotFound();
    entity.IsRead = true;
    entity.ReadAt = DateTimeOffset.UtcNow;
    entity.ModifiedDate = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(new { success = true });
});

app.MapPut("/api/notifications/read-all", async ([FromQuery] string? userId, NotificationDbContext db = null!) =>
{
    var query = db.Notifications.Where(n => !n.Deleted && !n.IsRead);
    if (!string.IsNullOrWhiteSpace(userId)) query = query.Where(n => n.RecipientUserId == userId);
    foreach (var entity in await query.ToListAsync())
    {
        entity.IsRead = true;
        entity.ReadAt = DateTimeOffset.UtcNow;
        entity.ModifiedDate = DateTimeOffset.UtcNow;
    }
    await db.SaveChangesAsync();
    return Results.Ok(new { success = true });
});

app.MapDelete("/api/notifications/{id}", async (int id, NotificationDbContext db = null!) =>
{
    var entity = await db.Notifications.FirstOrDefaultAsync(n => n.Id == id && !n.Deleted);
    if (entity is null) return Results.NotFound();
    entity.Deleted = true;
    entity.DeletedDate = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(new { success = true });
});

SeedNotificationDatabase(app);
app.Run();

static void SeedNotificationDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    if (db.Database.GetMigrations().Any())
    {
        db.Database.Migrate();
    }
    else
    {
        db.Database.EnsureCreated();
    }

    if (!db.NotificationTypes.Any())
    {
        db.NotificationTypes.AddRange(
            new NotificationType { Id = 1, Name = "System", Description = "System alerts and maintenance notices", Icon = "notifications", Color = "#DD9024", CreatedDate = DateTimeOffset.UtcNow },
            new NotificationType { Id = 2, Name = "Success", Description = "Success messages and confirmations", Icon = "check_circle", Color = "#16A34A", CreatedDate = DateTimeOffset.UtcNow },
            new NotificationType { Id = 3, Name = "Info", Description = "Informational updates", Icon = "info", Color = "#2563EB", CreatedDate = DateTimeOffset.UtcNow },
            new NotificationType { Id = 4, Name = "Comment", Description = "Comments and feedback from reviewers", Icon = "chat_bubble", Color = "#7C3AED", CreatedDate = DateTimeOffset.UtcNow }
        );
    }

    if (!db.NotificationPreferences.Any())
    {
        db.NotificationPreferences.Add(new NotificationPreference
        {
            Id = 1,
            UserId = "staff-user",
            EmailEnabled = true,
            InAppEnabled = true,
            NotificationTypes = "all",
            CreatedDate = DateTimeOffset.UtcNow
        });
    }

    if (!db.Notifications.Any())
    {
        db.Notifications.AddRange(
            new Notification
            {
                Id = 1,
                Title = "Idea Submitted",
                Message = "Your idea \"Digital KYC Blockchain Ledger\" has been submitted successfully.",
                Type = "success",
                RecipientUserId = "staff-user",
                IsRead = false,
                ActionUrl = "#",
                CreatedDate = DateTimeOffset.UtcNow.AddDays(-1)
            },
            new Notification
            {
                Id = 2,
                Title = "Status Update",
                Message = "Your idea \"Micro-Loan AI Risk Engine\" has moved to \"Experimentation\" stage.",
                Type = "info",
                RecipientUserId = "staff-user",
                IsRead = false,
                ActionUrl = "#",
                CreatedDate = DateTimeOffset.UtcNow.AddDays(-2)
            },
            new Notification
            {
                Id = 3,
                Title = "Idea Approved",
                Message = "Congratulations! Your idea \"Cashless Marketplace Initiative\" has been approved.",
                Type = "success",
                RecipientUserId = "staff-user",
                IsRead = true,
                ActionUrl = "#",
                CreatedDate = DateTimeOffset.UtcNow.AddDays(-5)
            }
        );
    }

    db.SaveChanges();
}

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationType> NotificationTypes => Set<NotificationType>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RecipientUserId).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ActionUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<NotificationType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<NotificationPreference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(100).IsRequired();
        });
    }
}

