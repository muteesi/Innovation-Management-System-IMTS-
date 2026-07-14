namespace IMTS.Shared.Models;

public class ResourceCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "description";
    public int SortOrder { get; set; }
}
