namespace IMTS.Shared.Models;

public class ResourceVersion : AuditableEntity
{
    public int ResourceId { get; set; }
    public string VersionNumber { get; set; } = "1.0";
    public string FileName { get; set; } = string.Empty;
    public string? StoragePath { get; set; }
    public long FileSizeBytes { get; set; }
    public string ChangeSummary { get; set; } = string.Empty;
}
