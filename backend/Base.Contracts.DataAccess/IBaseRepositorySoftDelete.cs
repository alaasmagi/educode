namespace Base.Contracts.DataAccess;

public interface IBaseRepositorySoftDelete<TEntity> : IBaseRepositorySoftDelete<TEntity, Guid, Guid> where TEntity : class
{
}

public interface IBaseRepositorySoftDelete<TEntity, TResourceKey, TUserKey> : IBaseRepository<TEntity, TResourceKey, TUserKey>
    where TEntity : class
    where TResourceKey : IEquatable<TResourceKey>
    where TUserKey : IEquatable<TUserKey>
{
    Task<IEnumerable<TEntity>?> GetAllAsync(bool includeSoftDeleted = false, TUserKey? userId = default!);
    Task<IEnumerable<TEntity>?> GetAllByPageAsync(int pageNr, int pageSize, bool includeSoftDeleted = false, TUserKey? userId = default!);
    Task<int> GetCountAsync(bool includeSoftDeleted = false, TUserKey? userId = default!);
    Task<TEntity?> GetByIdAsync(TResourceKey id, bool includeSoftDeleted = false, TUserKey? userId = default!);
    Task<bool> ExistsAsync(TResourceKey id, bool includeSoftDeleted = false, TUserKey? userId = default!);
    Task<bool> SoftDeleteAsync(TResourceKey id, TUserKey? userId = default!);
    Task<TEntity?> RestoreAsync(TResourceKey id, TUserKey? userId = default!);
}
