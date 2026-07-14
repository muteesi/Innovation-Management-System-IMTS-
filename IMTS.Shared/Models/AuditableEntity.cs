namespace IMTS.Shared.Models;

public abstract class AuditableEntity
{
    public int Id { get; set; }
    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;
    public string CreatedBy { get; set; } = "system";
    public DateTimeOffset? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public bool Deleted { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
}
