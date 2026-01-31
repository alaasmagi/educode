using App.Common;
using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF;

public class UserAuthRepository(AppDbContext context, ILogger<UserAuthRepository> logger, SentryService sentry) : IUserAuthRepository
{
    public async Task<List<UserAuthEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.UserAuthData
                    .IgnoreQueryFilters()
                    .OrderBy(ua => ua.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.UserAuthData
                    .OrderBy(ua => ua.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user auth data. Page: {PageNr}, Size: {PageSize}", pageNr, pageSize);
            sentry.CaptureWithContext(ex, "Error retrieving user auth data. Page: {0}, Size: {1}", pageNr, pageSize);
            return null;
        }    }

    public async Task<UserAuthEntity?> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.UserAuthData
                    .Include(ua => ua.User)
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(ua => ua.Id == id) : 
                await context.UserAuthData
                    .Include(ua => ua.User)
                    .FirstOrDefaultAsync(ua => ua.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user auth data. UserAuth ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving user auth data. UserAuth ID: {0}", id);
            return null;
        }
    }

    public async Task<UserAuthEntity?> CreateAsync(UserAuthEntity entity)
    {
        try
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            await context.UserAuthData.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error creating user auth data. User ID: {UserId}", entity.UserId);
            sentry.CaptureWithContext(ex, "Database error creating user auth data. User ID: {0}", entity.UserId);
            return null;
        }
    }

    public async Task<UserAuthEntity?> UpdateAsync(UserAuthEntity entity)
    {
        try
        {
            var existingEntity = await context.UserAuthData.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;
            
            existingEntity.UserId = entity.UserId;
            existingEntity.PasswordHash = entity.PasswordHash;
            existingEntity.Verified = entity.Verified;
            existingEntity.Deleted= entity.Deleted;
            existingEntity.UpdatedAt = DateTime.UtcNow;
            existingEntity.UpdatedBy = entity.UpdatedBy;

            await context.SaveChangesAsync();
            return existingEntity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict updating user auth data. UserAuth ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating user auth data. UserAuth ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating user auth data. UserAuth ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating user auth data. UserAuth ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<UserAuthEntity?> RemoveAsync(UserAuthEntity entity)
    {
        try
        {
            context.UserAuthData.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error removing user auth data. UserAuth ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing user auth data. UserAuth ID: {0}", entity.Id);
            return null;
        }
    }
}