using System.ComponentModel.DataAnnotations;
using Base.Contracts.Domain;

namespace Base.Domain;

public abstract class BaseEntityWithMetaSoftDelete : BaseEntityWithMetaSoftDelete<Guid>
{
}

public abstract class BaseEntityWithMetaSoftDelete<TKey> : BaseEntity<TKey>, IBaseEntityWithMetaSoftDelete<TKey> where TKey : IEquatable<TKey>
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
    
    [Required]
    public virtual bool IsDeleted { get; set; } = false;
}