using System.Text.Json;
using App.BLL.Contracts;
using App.Common;
using App.DAL.Contracts;
using App.Domain;
using Microsoft.Extensions.Logging;

namespace App.BLL;

public class UserManagementService(
    IAuthService authService,
    IUserRepository userRepository,
    IUserAuthRepository userAuthRepository,
    IUserTypeRepository userTypeRepository,
    EnvInitializer envInitializer,
    ICacheRepository cacheRepository,
    ILogger<UserManagementService> logger) : IUserManagementService
{
    public async Task<UserEntity?> AuthenticateUserAsync(Guid userId, string password)
    {
        var userAuthData = await userAuthRepository.GetByUserAsync(userId);

        if (userAuthData == null)
        {
            logger.LogError($"Failed to fetch user auth data for user with ID {userId}");
            return null;
        }

        var result = authService.VerifyPassword(password, userAuthData.PasswordHash);
        
        if (!result)
        {
            logger.LogError($"Failed to authenticate user with ID {userId}");
            return null;
        }
        
        return userAuthData.User;
    }

    public async Task<bool> CreateAccountAsync(UserEntity user, UserAuthEntity userAuthData)
    {
        if (await DoesUserExistAsync(user.Email))
        {
            logger.LogError($"Failed to create account for user with email {user.Email}");
            return false;
        }

        if (user.StudentCode != null)
        {
            user.StudentCode = user.StudentCode.ToUpper();    
        }
        
        if (await userRepository.CreateAsync(user) == null)
        {
            logger.LogError($"Failed to create account for user with email {user.Email}");
            return false;
        }
        
        userAuthData.UserId = user.Id;
        if (await userAuthRepository.CreateAsync(userAuthData) == null)
        {
            logger.LogError($"Failed to create account for user with email {user.Email}");
            return false;
        }

        return true;
    }

    public async Task<bool> ChangeUserPasswordAsync(UserEntity user, string newPasswordHash)
    { 
        return true;
    }

    public async Task<bool> DoesUserExistAsync(string email)
    {
        var status = await userRepository.CheckAvailabilityByEmailAsync(email);
        
        if (status == null)
        {
            logger.LogError($"User with email {email} was not found");
            return false;
        }
        
        logger.LogInformation($"User with email {email} was found");
        return true;
    }
    
    public async Task<UserTypeEntity?> GetUserTypeAsync(string userType)
    {
        var cache = await cacheRepository.GetAsync(Constants.UserTypePrefix + userType);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<UserTypeEntity?>(cache);
        }
        
        var result = await userTypeRepository.GetByItselfAsync(userType);
        if (result == null)
        {
            logger.LogError($"Failed to get user type {userType}");
            return null;
        }
        
        var serializedUserType = JsonSerializer.Serialize(result);
        await cacheRepository.SetAsync(Constants.UserTypePrefix + userType, 
            serializedUserType, Constants.ExtraLongCachePeriod);
        
        return  result;
    }

    public async Task<List<UserEntity>?> GetAllUsersAsync(int pageNr, int pageSize)
    {
        var cache = await cacheRepository.GetAsync(Constants.UserPrefix + pageNr + pageSize);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<List<UserEntity>?>(cache);
        }

        var result = await userRepository.GetAllAsync(pageNr, pageSize);

        if (result == null)
        {
            logger.LogError("Failed to get all users");
            return null;
        }

        var serializedUsers = JsonSerializer.Serialize(result);
        await cacheRepository.SetAsync(
            Constants.UserPrefix + pageNr + pageSize,
            serializedUsers, Constants.ShortCachePeriod);

        return result;
        
    }

    public async Task<UserEntity?> GetUserByEmailAsync(string email)
    {
        var cache = await cacheRepository.GetAsync(Constants.UserPrefix + email);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<UserEntity?>(cache);
        }
        
        var userId = await userRepository.CheckAvailabilityByEmailAsync(email);
        
        if (userId == null)
        {
            logger.LogError($"User with email {email} not found");
            return null;
        }
        
        var result = await userRepository.GetByIdAsync(userId.Value);
        
        if (result == null)
        {
            logger.LogError($"User with email {email} not found");
            return null;
        }
        
        var serializedUser = JsonSerializer.Serialize(result);
        await cacheRepository.SetAsync(Constants.UserPrefix + email,
            serializedUser, Constants.DefaultCachePeriod);

        return result;
    }
    
    public async Task<UserEntity?> GetUserByIdAsync(Guid id)
    {
        var cache = await cacheRepository.GetAsync(Constants.UserPrefix + id);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<UserEntity?>(cache);
        }
        
        var result = await userRepository.GetByIdAsync(id);

        if (result == null)
        {
            logger.LogError($"User with ID {id} not found");
            return null;
        }
        
        var serializedUser = JsonSerializer.Serialize(result);
        await cacheRepository.SetAsync(Constants.UserPrefix + id,
            serializedUser, Constants.DefaultCachePeriod);

        return result;
    }
    
    public async Task<bool> DeleteUserAsync(UserEntity user)
    {
        await cacheRepository.DeletePatternAsync($"*{user.Id.ToString()}*");
        await cacheRepository.DeletePatternAsync($"*{user.Email}*");
        await cacheRepository.DeletePatternAsync($"*{user.Id}*");
        
        var status = await userRepository.RemoveAsync(user);
        if (status == null)
        {
            logger.LogError($"Failed to delete user with ID {user.Id}");
            return false;
        }

        logger.LogInformation($"Successfully deleted user with ID {user.Id}");
        return true;
    }
    
    // TODO: Implement Edit User method
    
    public async Task<bool> UpdateUserAsync(UserEntity user)
    {
        await cacheRepository.DeletePatternAsync($"*{user.Id.ToString()}*");
        await cacheRepository.DeletePatternAsync($"*{user.Email}*");
        
        var status = await userRepository.UpdateAsync(user);
        if (status == null)
        {
            logger.LogError($"Failed to update user with ID {user.Id}");
            return false;
        }

        logger.LogInformation($"Successfully updated user with ID {user.Id}");
        return true;
    }
    
    public Task SeedUserTypes()
    {   
        var now = DateTime.UtcNow;
        var userTypes = new List<UserTypeEntity>
        {
            new UserTypeEntity
            {
                TypeName = "student",
                AccessLevel = EAccessLevel.PrimaryLevel,
                CreatedBy = "aspnet-initializer",
                CreatedAt = now,
                UpdatedBy = "aspnet-initializer",
                UpdatedAt = now,
            },
            new UserTypeEntity
            {
                TypeName = "teacher-assistant",
                AccessLevel = EAccessLevel.SecondaryLevel,
                CreatedBy = "aspnet-initializer",
                CreatedAt = now,
                UpdatedBy = "aspnet-initializer",
                UpdatedAt = now,
            },
            new UserTypeEntity
            {
                TypeName = "teacher",
                AccessLevel = EAccessLevel.TertiaryLevel,
                CreatedBy = "aspnet-initializer",
                CreatedAt = now,
                UpdatedBy = "aspnet-initializer",
                UpdatedAt = now,
            },
            new UserTypeEntity
            {
                TypeName = "school-administrator",
                AccessLevel = EAccessLevel.QuaternaryLevel,
                CreatedBy = "aspnet-initializer",
                CreatedAt = now,
                UpdatedBy = "aspnet-initializer",
                UpdatedAt = now,
            },
            new UserTypeEntity
            {
                TypeName = "system-administrator",
                AccessLevel = EAccessLevel.QuinaryLevel,
                CreatedBy = "aspnet-initializer",
                CreatedAt = now,
                UpdatedBy = "aspnet-initializer",
                UpdatedAt = now,
            }
        };

        userTypeRepository.SeedUserTypes(userTypes);
        return Task.CompletedTask;
    }
    
    public async Task SeedAdminUser()
    {   
        var now = DateTime.UtcNow;
        
        var adminUserTypes = await userTypeRepository.GetTypeByLevelAsync(EAccessLevel.QuinaryLevel);
        
        if (adminUserTypes == null || adminUserTypes.Count == 0 || adminUserTypes[0].Id == Guid.Empty)
        {
            return;
        }
        
        var existingAdminUser = await userRepository.CheckAvailabilityByFullNameAsync(envInitializer.DefaultAdminUser);
        
        if (existingAdminUser != null)
        {
            return;
        }
        
        var adminUser = new UserEntity
        {
            Email = envInitializer.DefaultAdminUser,
            TypeId = adminUserTypes[0].Id,
            FullName = envInitializer.DefaultAdminUser,
            CreatedBy = "aspnet-initializer",
            CreatedAt = now,
            UpdatedBy = "aspnet-initializer",
            UpdatedAt = now
        };
        
        var adminAuth = new UserAuthEntity
        {
            UserId = adminUser.Id,
            PasswordHash = authService.HashPassword(envInitializer.DefaultAdminPassword),
            
            CreatedBy = "aspnet-initializer",
            CreatedAt = now,
            UpdatedBy = "aspnet-initializer",
            UpdatedAt = now
        };
        
        await userRepository.CreateAsync(adminUser);
        await userAuthRepository.CreateAsync(adminAuth);
    }
}
    
    /* TODO: Implement soft deletion that cascade-soft-deletes UserAuthData, CourseTeachers, Courses, AttendanceChecks
                and HARD-deletes all User's RefreshTokens */

    
    // TODO: Implement an authentication method that can authenticate soft deleted users (IgnoreQueryFilers)
    
    // TODO: Implement restoration method that cascade-restores UserAuthData, CourseTeachers, Courses, AttendanceChecks