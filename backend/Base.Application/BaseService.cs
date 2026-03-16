using Base.Contracts.Application;
using Base.Contracts.DataAccess;
using Base.Contracts.Domain;

namespace Base.Application;

public class BaseService<TEntity, TRepository, TKey, TUserKey> : IBaseService<TEntity, TKey, TUserKey> 
    where TEntity : class, IBaseEntity<TKey> 
    where TRepository : class, IBaseRepository<TEntity, TKey, TUserKey> 
    where TKey : IEquatable<TKey> 
    where TUserKey : IEquatable<TUserKey>
{
    protected readonly IBaseUow ServiceUow;
    protected readonly TRepository ServiceRepository;
    
    public BaseService(IBaseUow serviceUow, TRepository serviceRepository)
    {
        ServiceUow = serviceUow;
        ServiceRepository = serviceRepository;
    }

    public async Task<IEnumerable<TEntity>?> GetAllAsync(TUserKey? userId = default)
    {
        return await ServiceRepository.GetAllAsync(userId);
    }

    public async Task<IEnumerable<TEntity>?> GetAllByPageAsync(int pageNr, int pageSize, TUserKey? userId = default)
    {
        return await ServiceRepository.GetAllByPageAsync(pageNr, pageSize, userId);
    }

    public async Task<int> GetCountAsync(TUserKey? userId = default)
    {
        return await ServiceRepository.GetCountAsync(userId);
    }

    public async Task<bool> ExistsAsync(TKey id, TUserKey? userId = default)
    {
        return await ServiceRepository.ExistsAsync(id, userId);
    }

    public async Task<TEntity?> GetByIdAsync(TKey id, TUserKey? userId = default)
    {
        return await ServiceRepository.GetByIdAsync(id, userId);
    }

    public async Task<TEntity?> CreateAsync(TEntity entity, TUserKey? userId = default)
    {
        var createdEntity = await ServiceRepository.CreateAsync(entity, userId);
        await ServiceUow.SaveChangesAsync();
        return createdEntity;
    }

    public async Task<TEntity?> UpdateAsync(TKey id, TEntity entity, TUserKey? userId = default)
    {
        var updatedEntity = await ServiceRepository.UpdateAsync(id, entity, userId);

        if (updatedEntity == null)
        {
            return null;
        }

        await ServiceUow.SaveChangesAsync();
        return updatedEntity;
    }

    public async Task<bool> RemoveAsync(TKey id, TUserKey? userId = default)
    {
        var removed = await ServiceRepository.RemoveAsync(id, userId);

        if (!removed)
        {
            return false;
        }

        await ServiceUow.SaveChangesAsync();
        return true;
    }
}
