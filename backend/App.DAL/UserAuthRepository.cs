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

public class UserAuthRepository(AppDbContext context, ILogger<UserAuthRepository> logger, SentryService sentry) : IUserAuthRepository
{
    public async Task<List<UserAuthEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.UserAuthData
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(ua => ua.User)
                    .OrderBy(ua => ua.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.UserAuthData
                    .AsNoTracking()
                    .Include(ua => ua.User)
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

    public async Task<int> CountAsync(bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.UserAuthData
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .CountAsync() : 
                await context.UserAuthData
                    .AsNoTracking()
                    .CountAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error counting user auth data");
            sentry.CaptureWithContext(ex, "Error counting user auth data");
            return 0;
        }
    }

    public async Task<UserAuthEntity?> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.UserAuthData
                    .Include(ua => ua.User)
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ua => ua.Id == id) : 
                await context.UserAuthData
                    .Include(ua => ua.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ua => ua.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user auth data. UserAuth ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving user auth data. UserAuth ID: {0}", id);
            return null;
        }
    }
    
    public async Task<UserAuthEntity?> GetByUser(Guid userId, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.UserAuthData
                    .Include(ua => ua.User)
                        .ThenInclude(u => u!.Type)
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ua => ua.UserId == userId) : 
                await context.UserAuthData
                    .Include(ua => ua.User)
                        .ThenInclude(u => u!.Type)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ua => ua.UserId == userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user auth data. User ID: {UserId}", userId);
            sentry.CaptureWithContext(ex, "Error retrieving user auth data. User ID: {0}", userId);
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
            var exists = await context.UserAuthData.IgnoreQueryFilters().AsNoTracking().AnyAsync(ua => ua.Id == entity.Id);
            if (!exists)
                return null;
        
            entity.UpdatedAt = DateTime.UtcNow;
            
            context.UserAuthData.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        
            await context.SaveChangesAsync();
            return entity;
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
            var entry = context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                context.UserAuthData.Attach(entity);
            }
        
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