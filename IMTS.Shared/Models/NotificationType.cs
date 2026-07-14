namespace IMTS.Shared.Models;

public class NotificationType : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "notifications";
    public string Color { get; set; } = "#DD9024";
}
