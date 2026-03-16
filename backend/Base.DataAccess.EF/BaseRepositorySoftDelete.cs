using Base.Contracts.DataAccess;
using Base.Contracts.Domain;
using Microsoft.EntityFrameworkCore;

namespace Base.DataAccess.EF;

public class BaseRepositorySoftDelete<TResourceEntity, TResourceKey, TUserKey> :
    BaseRepository<TResourceEntity, TResourceKey, TUserKey>,
    IBaseRepositorySoftDelete<TResourceEntity, TResourceKey, TUserKey>
    where TResourceEntity : class, IBaseEntity<TResourceKey>, IBaseEntitySoftDelete
    where TResourceKey : IEquatable<TResourceKey>
    where TUserKey : IEquatable<TUserKey>
{
    public BaseRepositorySoftDelete(DbContext repositoryDbContext) : base(repositoryDbContext)
    {
    }

    protected virtual IQueryable<TResourceEntity> GetQuery(bool includeSoftDeleted = false, TUserKey? userId = default!, bool asTracking = false)
    {
        var query = base.GetQuery(userId, asTracking);

        if (!includeSoftDeleted)
        {
            query = query.Where(entity => !entity.IsDeleted);
        }

        return query;
    }

    public async Task<IEnumerable<TResourceEntity>?> GetAllAsync(bool includeSoftDeleted = false, TUserKey? userId = default!)
    {
        return await GetQuery(includeSoftDeleted, userId).ToListAsync();
    }

    public async Task<IEnumerable<TResourceEntity>?> GetAllByPageAsync(int pageNr, int pageSize, bool includeSoftDeleted = false, TUserKey? userId = default!)
    {
        ValidatePaging(pageNr, pageSize);

        return await GetQuery(includeSoftDeleted, userId)
            .Skip((pageNr - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(bool includeSoftDeleted = false, TUserKey? userId = default!)
    {
        return await GetQuery(includeSoftDeleted, userId).CountAsync();
    }

    public async Task<TResourceEntity?> GetByIdAsync(TResourceKey id, bool includeSoftDeleted = false, TUserKey? userId = default!)
    {
        return await GetQuery(includeSoftDeleted, userId)
            .FirstOrDefaultAsync(entity => entity.Id.Equals(id));
    }

    public async Task<bool> ExistsAsync(TResourceKey id, bool includeSoftDeleted = false, TUserKey? userId = default!)
    {
        return await GetQuery(includeSoftDeleted, userId)
            .AnyAsync(entity => entity.Id.Equals(id));
    }

    public async Task<bool> SoftDeleteAsync(TResourceKey id, TUserKey? userId = default!)
    {
        var entity = await GetQuery(true, userId, asTracking: true)
            .FirstOrDefaultAsync(resourceEntity => resourceEntity.Id.Equals(id));

        if (entity == null || entity.IsDeleted)
        {
            return false;
        }

        entity.IsDeleted = true;
        ApplyModificationMetadata(entity, userId);
        return true;
    }

    public async Task<TResourceEntity?> RestoreAsync(TResourceKey id, TUserKey? userId = default!)
    {
        var entity = await GetQuery(true, userId, asTracking: true)
            .FirstOrDefaultAsync(resourceEntity => resourceEntity.Id.Equals(id));

        if (entity == null || !entity.IsDeleted)
        {
            return null;
        }

        entity.IsDeleted = false;
        ApplyModificationMetadata(entity, userId);
        return entity;
    }
}
