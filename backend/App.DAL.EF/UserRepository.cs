using App.Domain;
using Microsoft.EntityFrameworkCore;

namespace App.DAL.EF;

public class UserRepository (AppDbContext context)
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
    
    public void SeedUserTypes()
    {
        if (!context.UserTypes.Any())
        {
            var now = DateTime.UtcNow;

            var userTypes = new List<UserTypeEntity>
            {
                new UserTypeEntity
                {
                    UserType = "student",
                    AccessLevel = EAccessLevel.PrimaryLevel,
                    CreatedBy = "aspnet-initializer",
                    CreatedAt = now,
                    UpdatedBy = "aspnet-initializer",
                    UpdatedAt = now,
                },
                new UserTypeEntity
                {
                    UserType = "teacher-assistant",
                    AccessLevel = EAccessLevel.SecondaryLevel,
                    CreatedBy = "aspnet-initializer",
                    CreatedAt = now,
                    UpdatedBy = "aspnet-initializer",
                    UpdatedAt = now,
                },
                new UserTypeEntity
                {
                    UserType = "teacher",
                    AccessLevel = EAccessLevel.TertiaryLevel,
                    CreatedBy = "aspnet-initializer",
                    CreatedAt = now,
                    UpdatedBy = "aspnet-initializer",
                    UpdatedAt = now,
                },
                new UserTypeEntity
                {
                    UserType = "school-administrator",
                    AccessLevel = EAccessLevel.QuaternaryLevel,
                    CreatedBy = "aspnet-initializer",
                    CreatedAt = now,
                    UpdatedBy = "aspnet-initializer",
                    UpdatedAt = now,
                }
            };
            
            context.UserTypes.AddRange(userTypes);
            context.SaveChanges();
        }
    }
}