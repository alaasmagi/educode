using System.ComponentModel.DataAnnotations;
using Base.Contracts.Domain;

namespace Base.Domain;

public abstract class BaseEntityWithMeta : BaseEntityWithMeta<Guid>
{
}

public abstract class BaseEntityWithMeta<TKey> : BaseEntity<TKey>, IBaseEntityWithMeta<TKey> where TKey : IEquatable<TKey>
{
    [Required]
    [MaxLength(128)]
    public virtual string CreatedBy { get; set; } = default!;
    
    [Required]
    public virtual DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Required]
    [MaxLength(128)]
    public virtual string UpdatedBy { get; set; } = default!;
    
    [Required]
    public virtual DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}