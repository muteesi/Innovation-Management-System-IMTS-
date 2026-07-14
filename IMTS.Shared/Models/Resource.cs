namespace IMTS.Shared.Models;

public class Resource : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "policy";
    public string FileType { get; set; } = "PDF";
    public string FileName { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public string? StoragePath { get; set; }
    public long FileSizeBytes { get; set; }
    public int DownloadCount { get; set; }
    public string Status { get; set; } = "Published";
    public string Version { get; set; } = "1.0";
    public bool IsFeatured { get; set; }
    public string? PreviewMetadata { get; set; }
    public DateTimeOffset? PublishedDate { get; set; }
}
