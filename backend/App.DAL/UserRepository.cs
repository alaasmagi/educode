using App.Common;
using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF;

public class UserRepository(AppDbContext context, ILogger<UserRepository> logger, SentryService sentry) : IUserRepository
{
    public async Task<List<UserEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Users
                    .IgnoreQueryFilters()
                    .OrderBy(u => u.Id)
                    .Skip((pageNr - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync() : 
                await context.Users
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

    public async Task<UserEntity?> GetByIdAsync(Guid id, bool includeDeleted)
    {
        try
        {
            return includeDeleted ? 
                await context.Users
                    .Include(u => u.UserType)
                    .Include(u => u.School)
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.Id == id) : 
                await context.Users
                    .Include(u => u.UserType)
                    .Include(u => u.School)
                    .FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user. User ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving user. User ID: {0}", id);
            return null;
        }    
    }

    // TODO: INDEXING!
    public async Task<List<UserEntity>?> SearchAsync(string keyword, Guid? resourceFilterId, bool includeDeleted)
    {
        try
        {
            var normalizedKeyword = keyword.ToLower().Trim();

            return includeDeleted ? 
                await context.Users
                    .IgnoreQueryFilters()
                    .Where(u =>
                        u.Email.ToLower().Contains(normalizedKeyword) ||
                        u.FullName.ToLower().Contains(normalizedKeyword) ||
                        u.StudentCode!.ToLower().Contains(normalizedKeyword))
                    .OrderBy(ut => ut.UserType)
                    .ToListAsync() : 
                await context.Users
                    .Where(u =>
                        u.Email.ToLower().Contains(normalizedKeyword) ||
                        u.FullName.ToLower().Contains(normalizedKeyword) ||
                        u.StudentCode!.ToLower().Contains(normalizedKeyword))
                    .OrderBy(ut => ut.UserType)
                    .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching users. Keyword: {Keyword}", keyword);
            sentry.CaptureWithContext(ex, "Error searching users. Keyword: {0}", keyword);
            return null;
        }
    }

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
            var existingEntity = await context.Users.FindAsync(entity.Id);
            if (existingEntity == null)
                return null;
            
            existingEntity.Email= entity.Email;
            existingEntity.FullName= entity.FullName;
            existingEntity.StudentCode= entity.StudentCode;
            existingEntity.PhotoPath= entity.PhotoPath;
            existingEntity.SchoolId= entity.SchoolId;
            existingEntity.UserTypeId= entity.UserTypeId;
            existingEntity.Deleted= entity.Deleted;
            existingEntity.UpdatedAt = DateTime.UtcNow;
            existingEntity.UpdatedBy = entity.UpdatedBy;

            await context.SaveChangesAsync();
            return existingEntity;
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
    
    public async Task<UserEntity?> RemoveAsync(UserEntity entity)
    {
        try
        {
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
                    .Where(u => u.Email == email)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync() :
                await context.Users
                    .Where(u => u.Email == email)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking user availability. Email: {Email}", email);
            sentry.CaptureWithContext(ex, "Error checking user availability. Email: {0}", email);
            return null;
        }
    }
    
    
    // TODO: MOVE THE MAIN SEEDING LOGIC TO THE SERVICE
    public void SeedAdminUser(UserEntity adminUser, UserAuthEntity adminAuth)
    {
        var adminExists = context.Users
            .Any(u => u.UserTypeId == adminUser.UserTypeId);
        
        if (adminExists)
        {
            return;
        }
        
        context.Users.Add(adminUser);        
        context.SaveChanges();

        
        context.UserAuthData.Add(adminAuth);
        context.SaveChanges();
    }
    

}