using Base.Contracts.DataAccess;
using Base.Contracts.Domain;

namespace Base.Contracts.Application;

public interface IBaseService<TEntity> : IBaseService<TEntity, Guid, Guid> where TEntity : class, IBaseEntity
{
}

public interface IBaseService<TEntity, TKey, TUserKey> : IBaseRepository<TEntity, TKey, TUserKey>
                                                                                    where TEntity : class, IBaseEntity<TKey>
                                                                                    where TKey : IEquatable<TKey>
                                                                                    where TUserKey : IEquatable<TUserKey>
{
    
}
