namespace App.Contracts.Repositories;

/*
 * This is a generic repository interface defining standard CRUD operations.
 * It can be implemented for different entity types to provide a consistent data access pattern.
 * If return value is null, the operation was not successful.
 */
public interface IRepository<TEntity> where TEntity : class
{
    Task<List<TEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted = false);
    Task<int> CountAsync(bool includeDeleted = false);
    Task<TEntity?> GetByIdAsync(Guid id, bool includeDeleted = false);
    Task<TEntity?> CreateAsync(TEntity entity);
    Task<TEntity?> UpdateAsync(TEntity entity);
    Task<TEntity?> RemoveAsync(TEntity entity);
}