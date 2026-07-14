namespace IMTS.Shared.Models;

public class EmailQueueItem : AuditableEntity
{
    public string ToAddress { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string BodyHtml { get; set; } = string.Empty;
    public string? TemplateKey { get; set; }
    public string Status { get; set; } = "Queued";
    public int RetryCount { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
}
