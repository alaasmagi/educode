namespace Base.Domain;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CreatedBy { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public string UpdatedBy { get; set; } = default!;
    public DateTime UpdatedAt { get; set; }
    public bool Deleted { get; set; } = false;
}