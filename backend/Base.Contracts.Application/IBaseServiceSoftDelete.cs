using Base.Contracts.DataAccess;
using Base.Contracts.Domain;

namespace Base.Contracts.Application;

public interface IBaseServiceSoftDelete<TEntity> : IBaseServiceSoftDelete<TEntity, Guid, Guid>
    where TEntity : class, IBaseEntity, IBaseEntitySoftDelete
{
}

public interface IBaseServiceSoftDelete<TEntity, TKey, TUserKey> : IBaseService<TEntity, TKey, TUserKey>,
    IBaseRepositorySoftDelete<TEntity, TKey, TUserKey>
    where TEntity : class, IBaseEntity<TKey>, IBaseEntitySoftDelete
    where TKey : IEquatable<TKey>
    where TUserKey : IEquatable<TUserKey>
{
}
