using App.Contracts.Repositories;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Infrastructure.Sentry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.EFCore;

public class UserTypeRepository(AppDbContext context, ILogger<UserTypeRepository> logger, SentryService sentry) : IUserTypeRepository
{
    public async Task<List<UserTypeEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.UserTypes
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .OrderBy(ut => ut.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.UserTypes
                    .AsNoTracking()
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
    
    public async Task<List<UserTypeEntity>?> GetTypeByLevelAsync(EAccessLevel level)
    {
        try
        {
            return await context.UserTypes
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(ut => ut.AccessLevel == level)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user types by EAccessLevel. EAccessLevel: {AccessLevel}", level);
            sentry.CaptureWithContext(ex, "Error retrieving user types by EAccessLevel. EAccessLevel: {0}", level);
            return null;
        }    
    }

    public async Task<int> CountAsync(bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.UserTypes
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .CountAsync() : 
                await context.UserTypes
                    .AsNoTracking()
                    .CountAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error counting user types");
            sentry.CaptureWithContext(ex, "Error counting user types");
            return 0;
        }
    }

    public async Task<UserTypeEntity?> GetByIdAsync(Guid id, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.UserTypes
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ut => ut.Id == id) : 
                await context.UserTypes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ut => ut.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user type. UserType ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving user type. UserType ID: {0}", id);
            return null;
        }    
    }
    
    public async Task<UserTypeEntity?> GetByItselfAsync(string userType, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.UserTypes
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ut => ut.TypeName == userType) : 
                await context.UserTypes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ut => ut.TypeName == userType);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user type by itself. UserTpe: {UserType}", userType);
            sentry.CaptureWithContext(ex, "Error retrieving user type. UserTpe: {0}", userType);
            return null;
        }        
    }
    
    public async Task<List<UserTypeEntity>?> SearchAsync(string keyword, Guid? resourceFilterId, bool includeDeleted)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();

            return includeDeleted ? 
                await context.UserTypes
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(ut =>
                        ut.TypeName.ToLower().Contains(normalizedKeyword))
                    .OrderBy(ut => ut.TypeName)
                    .ToListAsync() : 
                await context.UserTypes
                    .AsNoTracking()
                    .Where(ut =>
                        ut.TypeName.ToLower().Contains(normalizedKeyword))
                    .OrderBy(ut => ut.TypeName)
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
            logger.LogError(ex, "Database error creating user type. UserType: {UserType}", entity.TypeName);
            sentry.CaptureWithContext(ex, "Database error creating user type. UserType: {0}", entity.TypeName);
            return null;
        }    
    }

    public async Task<UserTypeEntity?> UpdateAsync(UserTypeEntity entity)
    {
        try
        {
            var exists = await context.UserTypes.IgnoreQueryFilters().AsNoTracking().AnyAsync(ut => ut.Id == entity.Id);
            if (!exists)
                return null;
        
            entity.UpdatedAt = DateTime.UtcNow;
            
            context.UserTypes.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict updating user type. UserType ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating user type. UserType ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating user type. UserType ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating user type. UserType ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<bool> ToggleDeletionAsync(Guid id, bool newDeletionState)
    {
        try
        {
            var affectedRows = await context.UserTypes
                .IgnoreQueryFilters()
                .Where(ut => ut.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(ut => ut.Deleted, newDeletionState)
                    .SetProperty(ut => ut.UpdatedAt, DateTime.UtcNow));

            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error toggling deletion state for user type. ID: {Id}, New State: {NewState}", 
                id, newDeletionState);
            sentry.CaptureWithContext(ex, "Error toggling deletion state for user type. ID: {0}, New State: {1}", 
                id, newDeletionState);
            return false;
        }
    }

    public async Task<UserTypeEntity?> RemoveAsync(UserTypeEntity entity)
    {
        try
        {
            var entry = context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                context.UserTypes.Attach(entity);
            }
        
            context.UserTypes.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error removing user type. UserType ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing user type. UserType ID: {0}", entity.Id);
            return null;
        }
    }
    
    // TODO: MOVE THE MAIN SEEDING LOGIC TO THE SERVICE
    public void SeedUserTypes(List<UserTypeEntity> userTypes)
    {
        if (!context.UserTypes.Any())
        {
            context.UserTypes.AddRange(userTypes);
            context.SaveChanges();
        }
    }
}