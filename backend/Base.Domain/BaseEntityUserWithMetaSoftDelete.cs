using System.ComponentModel.DataAnnotations;
using Base.Contracts.Domain;

namespace Base.Domain;

public abstract class BaseEntityUserWithMetaSoftDelete : BaseEntityUserWithMetaSoftDelete<Guid, Guid>
{
}

public abstract class BaseEntityUserWithMetaSoftDelete<TKey, TUserKey> : BaseEntityWithMetaSoftDelete<TKey>, IBaseEntityUserId<TUserKey>
    where TKey : IEquatable<TKey>
    where TUserKey : IEquatable<TUserKey>
{
    [Required]
    public virtual TUserKey UserId { get; set; } = default!;
}
