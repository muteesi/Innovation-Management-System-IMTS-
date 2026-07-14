namespace IMTS.Shared.Models;

public class Notification : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "info";
    public string RecipientUserId { get; set; } = "staff-user";
    public bool IsRead { get; set; }
    public DateTimeOffset? ReadAt { get; set; }
    public string? ActionUrl { get; set; }
    public string? Metadata { get; set; }
}
