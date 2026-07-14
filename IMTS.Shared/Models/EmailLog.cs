namespace IMTS.Shared.Models;

public class EmailLog : AuditableEntity
{
    public string ToAddress { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Status { get; set; } = "Sent";
    public string? ErrorMessage { get; set; }
    public DateTimeOffset? SentAt { get; set; }
}
