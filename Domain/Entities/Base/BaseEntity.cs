namespace Domain.Entities.Base;
public abstract class BaseEntity : IAuditableEntity
{
    public DateTime CreatedAt { get; set; } 
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}