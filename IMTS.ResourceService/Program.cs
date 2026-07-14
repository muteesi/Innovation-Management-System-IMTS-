using IMTS.Shared.Dtos;
using IMTS.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ResourceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost;Database=InnovationManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.MapGet("/api/resources", async ([FromQuery] string? category, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 12, ResourceDbContext db = null!) =>
{
    IQueryable<Resource> query = db.Resources.AsNoTracking().Where(r => !r.Deleted).OrderByDescending(r => r.CreatedDate);

    if (!string.IsNullOrWhiteSpace(category) && category != "all")
        query = query.Where(r => r.Category == category);

    if (!string.IsNullOrWhiteSpace(search))
        query = query.Where(r => r.Title.Contains(search) || r.Description.Contains(search));

    var total = await query.CountAsync();
    var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

    return Results.Ok(new
    {
        items = items.Select(r => new
        {
            id = r.Id,
            title = r.Title,
            description = r.Description,
            category = r.Category,
            type = r.FileType,
            size = FormatSize(r.FileSizeBytes),
            icon = MapIcon(r.Category),
            downloads = r.DownloadCount,
            status = r.Status,
            fileUrl = r.FileUrl,
            previewMetadata = r.PreviewMetadata,
            publishedDate = r.PublishedDate
        }),
        totalCount = total,
        page,
        pageSize
    });
});

app.MapGet("/api/resources/stats", async (ResourceDbContext db = null!) =>
{
    var resources = await db.Resources.AsNoTracking().Where(r => !r.Deleted).ToListAsync();
    return Results.Ok(new ResourceStatsDto(resources.Count, resources.Count(r => r.Status == "Published"), resources.Count(r => r.Status == "Draft"), resources.Sum(r => r.DownloadCount)));
});

app.MapPost("/api/resources", async ([FromBody] ResourceCreateRequest request, ResourceDbContext db = null!) =>
{
    var entity = new Resource
    {
        Title = request.Title,
        Description = request.Description,
        Category = request.Category,
        FileType = request.FileType,
        FileName = request.FileName,
        FileUrl = request.FileUrl,
        StoragePath = request.StoragePath,
        FileSizeBytes = 0,
        Status = request.Status,
        Version = request.Version,
        PreviewMetadata = request.PreviewMetadata,
        Slug = request.Title.ToLowerInvariant().Replace(" ", "-")
    };

    db.Resources.Add(entity);
    await db.SaveChangesAsync();
    return Results.Created($"/api/resources/{entity.Id}", new { id = entity.Id });
});

app.MapPut("/api/resources/{id}", async (int id, [FromBody] ResourceUpdateRequest request, ResourceDbContext db = null!) =>
{
    var entity = await db.Resources.FirstOrDefaultAsync(r => r.Id == id && !r.Deleted);
    if (entity is null) return Results.NotFound();

    if (request.Title is not null) entity.Title = request.Title;
    if (request.Description is not null) entity.Description = request.Description;
    if (request.Category is not null) entity.Category = request.Category;
    if (request.FileType is not null) entity.FileType = request.FileType;
    if (request.FileName is not null) entity.FileName = request.FileName;
    if (request.FileUrl is not null) entity.FileUrl = request.FileUrl;
    if (request.StoragePath is not null) entity.StoragePath = request.StoragePath;
    if (request.Status is not null) entity.Status = request.Status;
    if (request.Version is not null) entity.Version = request.Version;
    if (request.PreviewMetadata is not null) entity.PreviewMetadata = request.PreviewMetadata;

    entity.ModifiedDate = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(new { success = true });
});

app.MapDelete("/api/resources/{id}", async (int id, ResourceDbContext db = null!) =>
{
    var entity = await db.Resources.FirstOrDefaultAsync(r => r.Id == id && !r.Deleted);
    if (entity is null) return Results.NotFound();
    entity.Deleted = true;
    entity.DeletedDate = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(new { success = true });
});

app.MapGet("/api/resources/download/{id}", async (int id, ResourceDbContext db = null!) =>
{
    var entity = await db.Resources.FirstOrDefaultAsync(r => r.Id == id && !r.Deleted);
    if (entity is null) return Results.NotFound();
    entity.DownloadCount++;
    entity.ModifiedDate = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(new { success = true, fileUrl = entity.FileUrl ?? $"/files/{entity.FileName}" });
});

SeedResourceDatabase(app);
app.Run();

static void SeedResourceDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ResourceDbContext>();
    if (db.Database.GetMigrations().Any())
    {
        db.Database.Migrate();
    }
    else
    {
        db.Database.EnsureCreated();
    }

    if (!db.Resources.Any())
    {
        db.Resources.AddRange(
            new Resource
            {
                Title = "Innovation Strategy 2024-2026",
                Description = "Comprehensive document outlining innovation management process and evaluation criteria.",
                Category = "policy",
                FileType = "PDF",
                FileName = "Innovation Strategy 2024-2026.pdf",
                FileUrl = "/files/Innovation-Strategy-2024-2026.pdf",
                StoragePath = null,
                FileSizeBytes = 4423680,
                Status = "Published",
                Version = "1.0",
                PreviewMetadata = null,
                Slug = "innovation-strategy-2024-2026",
                PublishedDate = DateTimeOffset.UtcNow.AddDays(-30),
                CreatedDate = DateTimeOffset.UtcNow.AddDays(-30)
            },
            new Resource
            {
                Title = "Project Budget Template V2",
                Description = "Excel template for measuring and documenting innovation impact with KPI dashboards.",
                Category = "templates",
                FileType = "XLSX",
                FileName = "Project Budget Template V2.xlsx",
                FileUrl = "/files/Project-Budget-Template-V2.xlsx",
                StoragePath = null,
                FileSizeBytes = 870400,
                Status = "Published",
                Version = "2.0",
                PreviewMetadata = null,
                Slug = "project-budget-template-v2",
                PublishedDate = DateTimeOffset.UtcNow.AddDays(-20),
                CreatedDate = DateTimeOffset.UtcNow.AddDays(-20)
            },
            new Resource
            {
                Title = "Innovation Pitch Deck 2024",
                Description = "Five-part video series covering the innovation lifecycle from ideation to implementation.",
                Category = "training",
                FileType = "PPTX",
                FileName = "Innovation Pitch Deck 2024.pptx",
                FileUrl = "/files/Innovation-Pitch-Deck-2024.pptx",
                StoragePath = null,
                FileSizeBytes = 13107200,
                Status = "Draft",
                Version = "1.0",
                PreviewMetadata = null,
                Slug = "innovation-pitch-deck-2024",
                PublishedDate = DateTimeOffset.UtcNow.AddDays(-10),
                CreatedDate = DateTimeOffset.UtcNow.AddDays(-10)
            }
        );
    }

    if (!db.ResourceCategories.Any())
    {
        db.ResourceCategories.AddRange(
            new ResourceCategory { Name = "Policy & Guidelines", Slug = "policy", Description = "Rules, policy documents and regulatory guidance.", Icon = "gavel", SortOrder = 1, CreatedDate = DateTimeOffset.UtcNow },
            new ResourceCategory { Name = "Templates", Slug = "templates", Description = "Reusable templates for innovation planning and reporting.", Icon = "description", SortOrder = 2, CreatedDate = DateTimeOffset.UtcNow },
            new ResourceCategory { Name = "Training & Guides", Slug = "training", Description = "Training materials and step-by-step guides.", Icon = "menu_book", SortOrder = 3, CreatedDate = DateTimeOffset.UtcNow }
        );
    }

    if (!db.ResourceVersions.Any())
    {
        db.ResourceVersions.Add(new ResourceVersion
        {
            ResourceId = 1,
            VersionNumber = "1.0",
            FileName = "Innovation Strategy 2024-2026.pdf",
            StoragePath = null,
            FileSizeBytes = 4423680,
            ChangeSummary = "Initial published version",
            CreatedDate = DateTimeOffset.UtcNow.AddDays(-30)
        });
    }

    db.SaveChanges();
}

string FormatSize(long bytes) => bytes switch
{
    < 1024 => $"{bytes} B",
    < 1024 * 1024 => $"{bytes / 1024} KB",
    _ => $"{bytes / (1024 * 1024)} MB"
};

string MapIcon(string category) => category switch
{
    "templates" => "table_chart",
    "training" => "menu_book",
    "research" => "folder_zip",
    "tools" => "insights",
    _ => "description"
};

public class ResourceDbContext : DbContext
{
    public ResourceDbContext(DbContextOptions<ResourceDbContext> options) : base(options) { }

    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<ResourceCategory> ResourceCategories => Set<ResourceCategory>();
    public DbSet<ResourceVersion> ResourceVersions => Set<ResourceVersion>();
    public DbSet<DownloadLog> DownloadLogs => Set<DownloadLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FileType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FileName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Version).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.Category);
        });

        modelBuilder.Entity<ResourceCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(50).IsRequired();
        });
    }
}

