using App.Common;
using App.DAL.Contracts;
using App.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.DAL.EF;

public class UserRepository(AppDbContext context, ILogger<UserRepository> logger, SentryService sentry) : IUserRepository
{
    public async Task<bool> AddUserEntityToDb(UserEntity newUser)
    {
        newUser.CreatedAt = DateTime.UtcNow;
        newUser.UpdatedAt = DateTime.UtcNow;
        
        await context.Users.AddAsync(newUser);
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> AddUserAuthEntityToDb(UserAuthEntity newUserAuth)
    {
        newUserAuth.CreatedAt = DateTime.UtcNow;
        newUserAuth.UpdatedAt = DateTime.UtcNow;
        
        await context.UserAuthData.AddAsync(newUserAuth);
        return await context.SaveChangesAsync() > 0;
    }
    
    public async Task<UserAuthEntity?> GetUserAuthDataByUserId(Guid userId)
    {
        return await context.UserAuthData
            .Include(ua => ua.User).ThenInclude(ua => ua!.UserType) 
            .FirstOrDefaultAsync(ua => ua.UserId == userId);
    }

    public async Task<bool> UpdateUserAuthEntity(Guid userId, string newPasswordHash)
    {
        var userAuth = await context.UserAuthData.FirstOrDefaultAsync(u => u.UserId == userId);

        if (userAuth == null)
        {
            return false;
        }
        
        userAuth.UpdatedAt = DateTime.UtcNow;
        userAuth.PasswordHash = newPasswordHash;
        
        return await context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteUserEntity(UserEntity user)
    {
        context.Users.Remove(user);
        return await context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> UpdateUserEntity(UserEntity user)
    {
        context.Users.Update(user);
        return await context.SaveChangesAsync() > 0;
    }


    public async Task<bool> UserAvailabilityCheckByEmail(string email)
    {
       return await context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<UserEntity?> GetUserByEmailAsync(string email)
    {
        return await context.Users.Include(x => x.UserType)
            .FirstOrDefaultAsync(x => x.Email == email);
    }
    
    public async Task<UserEntity?> GetUserByIdAsync(Guid userId)
    {
        return await context.Users.Include(u => u.UserType).FirstOrDefaultAsync(u => u.Id == userId);
    }
    
    public async Task<UserTypeEntity?> GetUserTypeEntity(string userType)
    {
        return await context.UserTypes.FirstOrDefaultAsync(u => u.UserType == userType);
    }

    public async Task<List<UserEntity>> GetAllUsersAsync(int pageNr, int pageSize)
    {
        return await context.Users
            .OrderBy(c => c.Id)
            .Skip((pageNr - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
    
    public async Task<bool> RemoveOldUsers(DateTime datePeriod)
    {
        return await context.Users
            .IgnoreQueryFilters()
            .Where(e => e.Deleted && e.UpdatedAt <= datePeriod)
            .ExecuteDeleteAsync() > 0;
    }
    
    public async Task<bool> RemoveOldUserTypes(DateTime datePeriod)
    {
        return await context.UserTypes
            .IgnoreQueryFilters()
            .Where(e => e.Deleted && e.UpdatedAt <= datePeriod)
            .ExecuteDeleteAsync() > 0;
    }
    
    public async Task<bool> RemoveOldUserAuths(DateTime datePeriod)
    {
        return await context.UserAuthData
            .IgnoreQueryFilters()
            .Where(e => e.Deleted && e.UpdatedAt <= datePeriod)
            .ExecuteDeleteAsync() > 0;
    }
    
    public async Task<bool> RemoveOldRefreshTokens(DateTime datePeriod)
    {
        return await context.RefreshTokens
            .IgnoreQueryFilters()
            .Where(e => e.Deleted && e.UpdatedAt <= datePeriod)
            .ExecuteDeleteAsync() > 0;
    }
    
    public async Task<bool> RemoveOldSchools(DateTime datePeriod)
    {
        return await context.Schools
            .IgnoreQueryFilters()
            .Where(e => e.Deleted && e.UpdatedAt <= datePeriod)
            .ExecuteDeleteAsync() > 0;
    }

    public Guid GetAdminUserTypeId()
    {
        var adminUserType = context.UserTypes
            .FirstOrDefault(ut => ut.AccessLevel == EAccessLevel.QuinaryLevel);

        return adminUserType?.Id ?? Guid.Empty;
    }
    
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
    
    public void SeedUserTypes(List<UserTypeEntity> userTypes)
    {
        if (!context.UserTypes.Any())
        {
            context.UserTypes.AddRange(userTypes);
            context.SaveChanges();
        }
    }
    
    
    

    public async Task<List<UserEntity>?> GetAllAsync(int pageNr, int pageSize, bool includeDeleted = false)
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

    public async Task<UserEntity?> GetByIdAsync(Guid id, bool includeDeleted = false)
    {
        try
        {
            return includeDeleted ? 
                await context.Users
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.Id == id) : 
                await context.Users
                    .FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving user. ID: {Id}", id);
            sentry.CaptureWithContext(ex, "Error retrieving user. ID: {0}", id);
            return null;
        }    
    }

    public async Task<List<UserEntity>?> SearchAsync(string keyword, bool includeDeleted = false)
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
            logger.LogError(ex, "Concurrency conflict updating user. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Concurrency conflict updating user. ID: {0}", entity.Id);
            return null;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error updating user. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error updating user. ID: {0}", entity.Id);
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
            logger.LogError(ex, "Database error removing user. ID: {Id}", entity.Id);
            sentry.CaptureWithContext(ex, "Database error removing user. ID: {0}", entity.Id);
            return null;
        }
    }
}