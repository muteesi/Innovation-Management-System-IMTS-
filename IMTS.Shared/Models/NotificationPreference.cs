namespace IMTS.Shared.Models;

public class NotificationPreference : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public bool EmailEnabled { get; set; } = true;
    public bool InAppEnabled { get; set; } = true;
    public string NotificationTypes { get; set; } = "all";
}
