namespace IMTS.Shared.Models;

public class DownloadLog : AuditableEntity
{
    public int ResourceId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
