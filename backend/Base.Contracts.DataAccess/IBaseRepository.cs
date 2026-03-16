namespace Base.Contracts.DataAccess;

public interface IBaseRepository<TEntity> : IBaseRepository<TEntity, Guid, Guid> where TEntity : class
{
}

public interface IBaseRepository<TEntity, TResourceKey, TUserKey> where TEntity : class 
                                                                where TResourceKey : IEquatable<TResourceKey>
                                                                where TUserKey : IEquatable<TUserKey>
{
    Task<IEnumerable<TEntity>?> GetAllAsync(TUserKey? userId = default!);
    Task<IEnumerable<TEntity>?> GetAllByPageAsync(int pageNr, int pageSize, TUserKey? userId = default!);
    Task<int> GetCountAsync(TUserKey? userId = default!);
    Task<bool> ExistsAsync(TResourceKey id, TUserKey? userId = default!);
    Task<TEntity?> GetByIdAsync(TResourceKey id, TUserKey? userId = default!);
    Task<TEntity?> CreateAsync(TEntity entity, TUserKey? userId = default!);
    Task<TEntity?> UpdateAsync(TResourceKey id, TEntity entity, TUserKey? userId = default!);
    Task<bool> RemoveAsync(TResourceKey id, TUserKey? userId = default!);
}
