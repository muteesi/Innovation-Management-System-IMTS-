namespace NotificationService.Models;

public class NotificationType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
