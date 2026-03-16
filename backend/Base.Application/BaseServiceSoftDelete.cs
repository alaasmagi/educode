using Base.Contracts.Application;
using Base.Contracts.DataAccess;
using Base.Contracts.Domain;

namespace Base.Application;

public class BaseServiceSoftDelete<TEntity, TRepository, TKey, TUserKey> :
    BaseService<TEntity, TRepository, TKey, TUserKey>,
    IBaseServiceSoftDelete<TEntity, TKey, TUserKey>
    where TEntity : class, IBaseEntity<TKey>, IBaseEntitySoftDelete
    where TRepository : class, IBaseRepositorySoftDelete<TEntity, TKey, TUserKey>
    where TKey : IEquatable<TKey>
    where TUserKey : IEquatable<TUserKey>
{
    public BaseServiceSoftDelete(IBaseUow serviceUow, TRepository serviceRepository) : base(serviceUow, serviceRepository)
    {
    }

    public async Task<IEnumerable<TEntity>?> GetAllAsync(bool includeSoftDeleted = false, TUserKey? userId = default)
    {
        return await ServiceRepository.GetAllAsync(includeSoftDeleted, userId);
    }

    public async Task<IEnumerable<TEntity>?> GetAllByPageAsync(int pageNr, int pageSize, bool includeSoftDeleted = false, TUserKey? userId = default)
    {
        return await ServiceRepository.GetAllByPageAsync(pageNr, pageSize, includeSoftDeleted, userId);
    }

    public async Task<int> GetCountAsync(bool includeSoftDeleted = false, TUserKey? userId = default)
    {
        return await ServiceRepository.GetCountAsync(includeSoftDeleted, userId);
    }

    public async Task<TEntity?> GetByIdAsync(TKey id, bool includeSoftDeleted = false, TUserKey? userId = default)
    {
        return await ServiceRepository.GetByIdAsync(id, includeSoftDeleted, userId);
    }

    public async Task<bool> ExistsAsync(TKey id, bool includeSoftDeleted = false, TUserKey? userId = default)
    {
        return await ServiceRepository.ExistsAsync(id, includeSoftDeleted, userId);
    }

    public async Task<bool> SoftDeleteAsync(TKey id, TUserKey? userId = default)
    {
        var deleted = await ServiceRepository.SoftDeleteAsync(id, userId);

        if (!deleted)
        {
            return false;
        }

        await ServiceUow.SaveChangesAsync();
        return true;
    }

    public async Task<TEntity?> RestoreAsync(TKey id, TUserKey? userId = default)
    {
        var restoredEntity = await ServiceRepository.RestoreAsync(id, userId);

        if (restoredEntity == null)
        {
            return null;
        }

        await ServiceUow.SaveChangesAsync();
        return restoredEntity;
    }
}
