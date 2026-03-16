using System.ComponentModel.DataAnnotations;
using Base.Contracts.Domain;

namespace Base.Domain;

public abstract class BaseEntityUser : BaseEntityUser<Guid>
{
}

public abstract class BaseEntityUser <TKey> : IBaseEntityUserId<TKey> where TKey : IEquatable<TKey>
{
    [Required]
    public TKey UserId { get; set; } = default!;
}