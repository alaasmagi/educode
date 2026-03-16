namespace Base.Contracts.Domain;

public interface IBaseEntityWithMetaSoftDelete : IBaseEntityWithMetaSoftDelete<Guid>
{
}

public interface IBaseEntityWithMetaSoftDelete<TKey> : IBaseEntity<TKey>, IBaseEntityMeta, IBaseEntitySoftDelete where TKey : IEquatable<TKey>
{
}