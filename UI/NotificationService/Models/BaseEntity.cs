using System.ComponentModel.DataAnnotations;

namespace NotificationService.Models;

public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public string? ModifiedBy { get; set; }
    public bool Deleted { get; set; }
    public DateTime? DeletedDate { get; set; }
}
