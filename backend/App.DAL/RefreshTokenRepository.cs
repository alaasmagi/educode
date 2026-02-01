using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                    .AsNoTracking()
                    .OrderBy(rt => rt.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.RefreshTokens
                    .AsNoTracking()
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
    
    public async Task<int> CountAsync(bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.RefreshTokens
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .CountAsync() : 
                await context.RefreshTokens
                    .AsNoTracking()
                    .CountAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error counting refresh tokens");
            sentry.CaptureWithContext(ex, "Error counting refresh tokens");
            return 0;
        }
    }
    
    public async Task<RefreshTokenEntity?> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.RefreshTokens
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(rt => rt.Id == id) : 
                await context.RefreshTokens
                    .AsNoTracking()
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
                .AsNoTracking()
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
            var exists = await context.RefreshTokens.IgnoreQueryFilters().AsNoTracking().AnyAsync(rt => rt.Id == entity.Id);
            if (!exists)
                return null;
        
            entity.UpdatedAt = DateTime.UtcNow;
            
            context.RefreshTokens.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        
            await context.SaveChangesAsync();
            return entity;
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
            var entry = context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                context.RefreshTokens.Attach(entity);
            }
        
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