using System.ComponentModel.DataAnnotations;
using Base.Contracts.Domain;

namespace Base.Domain;

public abstract class BaseEntityUserWithMeta : BaseEntityUserWithMeta<Guid, Guid>
{
}

public abstract class BaseEntityUserWithMeta<TKey, TUserKey> : BaseEntityWithMeta<TKey>, IBaseEntityUserId<TUserKey>
    where TKey : IEquatable<TKey>
    where TUserKey : IEquatable<TUserKey>
{
    [Required]
    public virtual TUserKey UserId { get; set; } = default!;
}
