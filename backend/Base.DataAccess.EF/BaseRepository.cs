using Base.Contracts.DataAccess;
using Base.Contracts.Domain;
using Microsoft.EntityFrameworkCore;

namespace Base.DataAccess.EF;

public class BaseRepository<TResourceEntity, TResourceKey, TUserKey> : IBaseRepository<TResourceEntity, TResourceKey, TUserKey>
                                                                            where TResourceEntity : class, IBaseEntity<TResourceKey>
                                                                            where TResourceKey : IEquatable<TResourceKey> 
                                                                            where TUserKey : IEquatable<TUserKey>
{
    protected readonly DbContext RepositoryDbContext;
    protected readonly DbSet<TResourceEntity> RepositoryDbSet;

    public BaseRepository(DbContext repositoryDbContext)
    {
        RepositoryDbContext = repositoryDbContext;
        RepositoryDbSet = RepositoryDbContext.Set<TResourceEntity>();
    }


    protected virtual IQueryable<TResourceEntity> GetQuery(TUserKey? userId = default!, bool asTracking = false)
    {
        var query = RepositoryDbSet.AsQueryable();

        if (!asTracking)
        {
            query = query.AsNoTracking();
        }

        if (ShouldUseUserId(userId))
        {
            query = query.Where(e => ((IBaseEntityUserId<TUserKey>)e).UserId.Equals(userId));
        }

        return query;
    }

    protected virtual void ValidatePaging(int pageNr, int pageSize)
    {
        if (pageNr <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNr), pageNr, "Page number must be greater than 0.");
        }

        if (pageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be greater than 0.");
        }
    }
    
    private bool ShouldUseUserId(TUserKey? userId = default!)
    {
        return typeof(IBaseEntityUserId<TUserKey>).IsAssignableFrom(typeof(TResourceEntity)) &&
               userId != null &&
               !EqualityComparer<TUserKey>.Default.Equals(userId, default);
    }

    private static bool HasMeta()
    {
        return typeof(IBaseEntityMeta).IsAssignableFrom(typeof(TResourceEntity));
    }

    private static string? GetUserIdentifier(TUserKey? userId = default!)
    {
        if (userId == null || EqualityComparer<TUserKey>.Default.Equals(userId, default!))
        {
            return null;
        }

        return userId.ToString();
    }

    protected virtual void ApplyCreateMetadata(TResourceEntity entity, TUserKey? userId = default!)
    {
        if (!HasMeta())
        {
            return;
        }

        var now = DateTime.UtcNow;
        var userIdentifier = GetUserIdentifier(userId);
        var metaEntity = (IBaseEntityMeta)entity;

        metaEntity.CreatedAt = now;
        metaEntity.UpdatedAt = now;

        if (!string.IsNullOrWhiteSpace(userIdentifier))
        {
            metaEntity.CreatedBy = userIdentifier;
            metaEntity.UpdatedBy = userIdentifier;
        }
    }

    protected virtual void ApplyUpdateMetadata(TResourceEntity entity, TResourceEntity existingEntity, TUserKey? userId = default!)
    {
        if (!HasMeta())
        {
            return;
        }

        var now = DateTime.UtcNow;
        var userIdentifier = GetUserIdentifier(userId);
        var metaEntity = (IBaseEntityMeta)entity;
        var existingMetaEntity = (IBaseEntityMeta)existingEntity;

        metaEntity.CreatedAt = existingMetaEntity.CreatedAt;
        metaEntity.CreatedBy = existingMetaEntity.CreatedBy;
        metaEntity.UpdatedAt = now;

        if (!string.IsNullOrWhiteSpace(userIdentifier))
        {
            metaEntity.UpdatedBy = userIdentifier;
        }
        else
        {
            metaEntity.UpdatedBy = existingMetaEntity.UpdatedBy;
        }
    }

    protected virtual void ApplyModificationMetadata(TResourceEntity entity, TUserKey? userId = default!)
    {
        if (!HasMeta())
        {
            return;
        }

        var userIdentifier = GetUserIdentifier(userId);
        var metaEntity = (IBaseEntityMeta)entity;
        metaEntity.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(userIdentifier))
        {
            metaEntity.UpdatedBy = userIdentifier;
        }
    }
    
    public async Task<IEnumerable<TResourceEntity>?> GetAllAsync(TUserKey? userId = default)
    {
        return await GetQuery(userId)
                .ToListAsync();
    }

    public async Task<IEnumerable<TResourceEntity>?> GetAllByPageAsync(int pageNr, int pageSize, TUserKey? userId = default)
    {
        ValidatePaging(pageNr, pageSize);

        return await GetQuery(userId)
                .Skip((pageNr - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();    
    }

    public async Task<int> GetCountAsync(TUserKey? userId = default)
    {
        var query = GetQuery(userId);
        return await query.CountAsync();
    }

    public async Task<bool> ExistsAsync(TResourceKey id, TUserKey? userId = default)
    {
        var query = GetQuery(userId);
        return await query.AnyAsync(e => e.Id.Equals(id));
    }

    public async Task<TResourceEntity?> GetByIdAsync(TResourceKey id, TUserKey? userId = default)
    {
        var query = GetQuery(userId);
        var res = await query.FirstOrDefaultAsync(e => e.Id.Equals(id));
        return res;
    }

    public Task<TResourceEntity?> CreateAsync(TResourceEntity entity, TUserKey? userId = default)
    {
        if (ShouldUseUserId(userId))
        {
            ((IBaseEntityUserId<TUserKey>)entity).UserId = userId!;
        }

        ApplyCreateMetadata(entity, userId);
        return Task.FromResult<TResourceEntity?>(RepositoryDbSet.Add(entity).Entity);
    }

    public async Task<TResourceEntity?> UpdateAsync(TResourceKey id, TResourceEntity entity, TUserKey? userId = default)
    {
        var dbEntity = await RepositoryDbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id.Equals(id));

        if (dbEntity == null)
        {
            return null;
        }

        if (ShouldUseUserId(userId))
        {
            if (!((IBaseEntityUserId<TUserKey>)dbEntity).UserId.Equals(userId))
            {
                return null;
            }

            ((IBaseEntityUserId<TUserKey>)entity).UserId = ((IBaseEntityUserId<TUserKey>)dbEntity).UserId;
        }

        ApplyUpdateMetadata(entity, dbEntity, userId);
        return RepositoryDbSet.Update(entity).Entity;
    }

    public async Task<bool> RemoveAsync(TResourceKey id, TUserKey? userId = default)
    {
        var query = GetQuery(userId, asTracking: true);
        query = query.Where(e => e.Id.Equals(id));
        var dbEntity = await query.FirstOrDefaultAsync();

        if (dbEntity == null)
        {
            return false;
        }

        RepositoryDbSet.Remove(dbEntity);
        return true;
    }
}
