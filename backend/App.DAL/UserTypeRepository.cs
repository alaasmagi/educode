using App.Common;
using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF;

public class UserTypeRepository(AppDbContext context, ILogger<UserTypeRepository> logger, SentryService sentry) : IUserTypeRepository
{
    public async Task<List<UserTypeEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.UserTypes
                    .IgnoreQueryFilters()
                    .OrderBy(ut => ut.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.UserTypes
                    .OrderBy(ut => ut.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user types. Page: {PageNr}, Size: {PageSize}", pageNr, pageSize);
            sentry.CaptureWithContext(ex, "Error retrieving user types. Page: {0}, Size: {1}", pageNr, pageSize);
            return null;
        }
    }

    public async Task<UserTypeEntity?> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.UserTypes
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(ut => ut.Id == id) : 
                await context.UserTypes
                    .FirstOrDefaultAsync(ut => ut.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user type. ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving user type. ID: {0}", id);
            return null;
        }    
    }
    
    // TODO: INDEXING!
    public async Task<List<UserTypeEntity>?> SearchAsync(string keyword, bool includeDeleted = false)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();

            return includeDeleted ? 
                await context.UserTypes
                    .IgnoreQueryFilters()
                    .Where(ut =>
                        ut.UserType.ToLower().Contains(normalizedKeyword))
                    .OrderBy(ut => ut.UserType)
                    .ToListAsync() : 
                await context.UserTypes
                    .Where(ut =>
                        ut.UserType.ToLower().Contains(normalizedKeyword))
                    .OrderBy(ut => ut.UserType)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching user types. Keyword: {Keyword}", keyword);
            sentry.CaptureWithContext(ex, "Error searching user types. Keyword: {0}", keyword);
            return null;
        }
    }

    public async Task<UserTypeEntity?> CreateAsync(UserTypeEntity entity)
    {
        try
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            await context.UserTypes.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error creating user type. UserType: {UserType}", entity.UserType);
            sentry.CaptureWithContext(ex, "Database error creating user type. UserType: {0}", entity.UserType);
            return null;
        }    
    }

    public async Task<UserTypeEntity?> UpdateAsync(UserTypeEntity entity)
    {
        try
        {
            var existingEntity = await context.UserTypes.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;
            
            existingEntity.UserType= entity.UserType;
            existingEntity.AccessLevel = entity.AccessLevel;
            existingEntity.Deleted= entity.Deleted;
            existingEntity.UpdatedAt = DateTime.UtcNow;
            existingEntity.UpdatedBy = entity.UpdatedBy;

            await context.SaveChangesAsync();
            return existingEntity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict updating user type. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating user type. ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating user type. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating user type. ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<UserTypeEntity?> RemoveAsync(UserTypeEntity entity)
    {
        try
        {
            context.UserTypes.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error removing user type. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing user type. ID: {0}", entity.Id);
            return null;
        }
    }
}