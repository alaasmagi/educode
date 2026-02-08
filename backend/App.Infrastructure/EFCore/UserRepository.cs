using App.Contracts.Repositories;
using App.Domain.Entities;
using App.Infrastructure.Sentry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.EFCore;

public class UserRepository(AppDbContext context, ILogger<UserRepository> logger, SentryService sentry) : IUserRepository
{
    public async Task<List<UserEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Users
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Include(u => u.Type)
                    .Include(u => u.School)
                    .OrderBy(u => u.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.Users
                    .AsNoTracking()
                    .Include(u => u.Type)
                    .Include(u => u.School)
                    .OrderBy(u => u.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving users. Page: {PageNr}, Size: {PageSize}", pageNr, pageSize);
            sentry.CaptureWithContext(ex, "Error retrieving users. Page: {0}, Size: {1}", pageNr, pageSize);
            return null;
        }    
    }

    public async Task<int> CountAsync(bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Users
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .CountAsync() : 
                await context.Users
                    .AsNoTracking()
                    .CountAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error counting users");
            sentry.CaptureWithContext(ex, "Error counting users");
            return 0;
        }
    }

    public async Task<UserEntity?> GetByIdAsync(Guid id, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Users
                    .Include(u => u.Type)
                    .Include(u => u.School)
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id) : 
                await context.Users
                    .Include(u => u.Type)
                    .Include(u => u.School)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user. User ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving user. User ID: {0}", id);
            return null;
        }    
    }

    public async Task<List<UserEntity>?> SearchAsync(string keyword, Guid? resourceFilterId, bool includeDeleted)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();

            return includeDeleted ? 
                await context.Users
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(u =>
                        u.Email.ToLower().Contains(normalizedKeyword) ||
                        u.FullName.ToLower().Contains(normalizedKeyword) ||
                        u.StudentCode!.ToLower().Contains(normalizedKeyword))
                    .OrderBy(u => u.Type)
                    .ToListAsync() : 
                await context.Users
                    .AsNoTracking()
                    .Where(u =>
                        u.Email.ToLower().Contains(normalizedKeyword) ||
                        u.FullName.ToLower().Contains(normalizedKeyword) ||
                        u.StudentCode!.ToLower().Contains(normalizedKeyword))
                    .OrderBy(u => u.Type)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching users. Keyword: {Keyword}", keyword);
            sentry.CaptureWithContext(ex, "Error searching users. Keyword: {0}", keyword);
            return null;
        }
    }

    public async Task<UserEntity?> GetByEmailAsync(string email, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Users
                    .Include(u => u.Type)
                    .Include(u => u.School)
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email) : 
                await context.Users
                    .Include(u => u.Type)
                    .Include(u => u.School)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user. User email: {Email}", email);
            sentry.CaptureWithContext(ex, "Error retrieving user. User email: {0}", email);
            return null;
        }        }

    public async Task<UserEntity?> CreateAsync(UserEntity entity)
    {
        try
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            
            await context.Users.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error creating user. Email: {UserType}", entity.Email);
            sentry.CaptureWithContext(ex, "Database error creating user. Email: {0}", entity.Email);
            return null;
        }
    }

    public async Task<UserEntity?> UpdateAsync(UserEntity entity)
    {
        try
        {
            var exists = await context.Users.IgnoreQueryFilters().AsNoTracking().AnyAsync(u => u.Id == entity.Id);
            if (!exists)
                return null;
        
            entity.UpdatedAt = DateTime.UtcNow;
            
            context.Users.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Concurrency conflict updating user. User ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating user. User ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating user. User ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating user. User ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<bool> ToggleDeletionAsync(Guid id, string email, string clientApp, bool newDeletionState)
    {
        try
        {
            var affectedRows = await context.Users
                .IgnoreQueryFilters()
                .Where(u => u.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.Deleted, newDeletionState)
                    .SetProperty(ac => ac.UpdatedBy, email)
                    .SetProperty(ac => ac.UpdatedByClient, clientApp)
                    .SetProperty(ua => ua.UpdatedAt, DateTime.UtcNow));

            return affectedRows > 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error toggling deletion state for user. ID: {Id}, New State: {NewState}", 
                id, newDeletionState);
            sentry.CaptureWithContext(ex, "Error toggling deletion state for user. ID: {0}, New State: {1}", 
                id, newDeletionState);
            return false;
        }
    }

    public async Task<UserEntity?> RemoveAsync(UserEntity entity)
    {
        try
        {
            var entry = context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                context.Users.Attach(entity);
            }
        
            context.Users.Remove(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error removing user. User ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing user. User ID: {0}", entity.Id);
            return null;
        }
    }

    public async Task<Guid?> CheckAvailabilityByEmailAsync(string email, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Users
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(u => u.Email == email)
                    .Select(u => (Guid?)u.Id)
                    .FirstOrDefaultAsync() :
                await context.Users
                    .AsNoTracking()
                    .Where(u => u.Email == email)
                    .Select(u => (Guid?)u.Id)
                    .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking user availability. Email: {Email}", email);
            sentry.CaptureWithContext(ex, "Error checking user availability. Email: {0}", email);
            return null;
        }
    }
    
    public async Task<Guid?> CheckAvailabilityByFullNameAsync(string fullName, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Users
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .Where(u => u.FullName == fullName)
                    .Select(u => (Guid?)u.Id)
                    .FirstOrDefaultAsync() :
                await context.Users
                    .AsNoTracking()
                    .Where(u => u.FullName == fullName)
                    .Select(u => (Guid?)u.Id)
                    .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking user availability. FullName: {FullName}", fullName);
            sentry.CaptureWithContext(ex, "Error checking user availability. FullName: {0}", fullName);
            return null;
        }
    }
}