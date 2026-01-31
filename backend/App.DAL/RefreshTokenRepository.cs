using App.Common;
using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF;

public class RefreshTokenRepository(AppDbContext context, ILogger<RefreshTokenRepository> logger, SentryService sentry)
                                                                                                : IRefreshTokenRepository
{
    public async Task<List<RefreshTokenEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.RefreshTokens
                    .IgnoreQueryFilters()
                    .OrderBy(rt => rt.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.RefreshTokens
                    .OrderBy(rt => rt.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving refresh tokens. Page: {PageNr}, Size: {PageSize}", pageNr, pageSize);
            sentry.CaptureWithContext(ex, "Error retrieving refresh tokens. Page: {0}, Size: {1}", pageNr, pageSize);
            return null;
        }
    }
    
    public async Task<RefreshTokenEntity?> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.RefreshTokens
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(rt => rt.Id == id) : 
                await context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving refresh token. RefreshToken ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving refresh token. RefreshToken ID: {0}", id);
            return null;
        }    
    }
    
    public async Task<RefreshTokenEntity?> GetByItself(string refreshToken)
    {
        try
        {
            return await context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving refresh token via token itself");
            sentry.CaptureWithContext(ex, "Error retrieving refresh token via token itself");
            return null;
        }
    }

    public async Task<RefreshTokenEntity?> CreateAsync(RefreshTokenEntity entity)
    {
        try
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            await context.RefreshTokens.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error creating refresh token. User ID: {UserId}", entity.UserId);
            sentry.CaptureWithContext(ex, "Database error creating refresh token. User ID: {0}", entity.UserId);
            return null;
        }
    }

    public async Task<RefreshTokenEntity?> UpdateAsync(RefreshTokenEntity entity)
    {
        try
        {
            var existingEntity = await context.RefreshTokens.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;
            
            existingEntity.UserId = entity.UserId;
            existingEntity.Token = entity.Token;
            existingEntity.Client = entity.Client;
            existingEntity.ClientIp = entity.ClientIp;
            existingEntity.ExpirationTime = entity.ExpirationTime;
            existingEntity.PushNotificationToken = entity.PushNotificationToken;
            existingEntity.Deleted= entity.Deleted;
            existingEntity.UpdatedAt = DateTime.UtcNow;
            existingEntity.UpdatedBy = entity.UpdatedBy;

            await context.SaveChangesAsync();
            return existingEntity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict updating refresh token. RefreshToken ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating refresh token. RefreshToken ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating refresh token. RefreshToken ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating refresh token. RefreshToken ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<RefreshTokenEntity?> RemoveAsync(RefreshTokenEntity entity)
    {
        try
        {
            context.RefreshTokens.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error removing refresh token. RefreshToken ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing refresh token. RefreshToken ID: {0}", entity.Id);
            return null;
        }    
    }
}