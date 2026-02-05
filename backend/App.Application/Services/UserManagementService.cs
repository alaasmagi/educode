using System.Text.Json;
using App.Application.Initializers;
using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using Microsoft.Extensions.Logging;

namespace App.Application.Services;

public class UserManagementService(
    IUserRepository userRepository,
    IPasswordService passwordService,
    ICourseTeacherRepository courseTeacherRepository,
    IAttendanceCheckRepository attendanceCheckRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUserAuthRepository userAuthRepository,
    IUserTypeRepository userTypeRepository,
    EnvInitializer envInitializer,
    ICacheRepository cacheRepository,
    ILogger<UserManagementService> logger) : IUserManagementService
{
    public async Task<UserEntity?> VerifyUser(string email, string otpEntry)
    {
        var user = await userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            logger.LogError($"Failed to fetch user with email {email}");
            return null;
        }
        
        var userAuthData = await userAuthRepository.GetByUserAsync(user.Id);
        if (userAuthData == null)
        {
            logger.LogError($"Failed to fetch user auth data for user with ID {user.Id}");
            return null;
        }
        
        if (!userAuthData.Verified)
        {
            logger.LogError($"User with ID {user.Id} is not verified");
            return null;
        }
        
        return userAuthData.User;
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
    
    public async Task<bool> SoftDeleteUserAsync(Guid userId)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
        {
            return false;
        }
        
        await cacheRepository.DeletePatternAsync($"*{user.Id.ToString()}*");
        await cacheRepository.DeletePatternAsync($"*{user.Email}*");
        await cacheRepository.DeletePatternAsync($"*{user.Id}*");
        
        await courseTeacherRepository.ToggleDeletionForAllByTeacherAsync(user.Id, true);
        await attendanceCheckRepository.ToggleDeletionForAllByUserAsync(user.FullName, true);
        await refreshTokenRepository.RemoveAllByUserAsync(user.Id);
        
        var userAuth = await userAuthRepository.GetByUserAsync(user.Id);
        if (await userRepository.ToggleDeletionAsync(user.Id, true) || 
            await userAuthRepository.ToggleDeletionAsync(userAuth!.Id, false))
        {
            logger.LogError($"Failed to delete user with ID {user.Id}");
            return false;
        }
        
        logger.LogInformation($"Successfully deleted user with ID {user.Id}");
        return true;
    }
    
    public async Task<bool> RestoreUserAsync(Guid userId)
    {
        await userRepository.ToggleDeletionAsync(userId, false);
        var user = await userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return false;
        }
        
        await courseTeacherRepository.ToggleDeletionForAllByTeacherAsync(user.Id, false);
        await attendanceCheckRepository.ToggleDeletionForAllByUserAsync(user.FullName, false);
        
        var userAuth = await userAuthRepository.GetByUserAsync(user.Id);
        if (await userRepository.ToggleDeletionAsync(user.Id, false) || 
            await userAuthRepository.ToggleDeletionAsync(userAuth!.Id, false))
        {
            logger.LogError($"Failed to restore user with ID {user.Id}");
            return false;
        }
        
        logger.LogInformation($"Successfully restored user with ID {user.Id}");
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
    
    public async Task<bool> SeedUserTypes()
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

        foreach (var userType in userTypes)
        {
            var result = await userTypeRepository.CreateAsync(userType);

            if (result == null)
            {
                // TODO: Implement proper error handling
                return false;
            }
        }

        // TODO: Implement proper logging
        return true;
    }
    
    public async Task<bool> SeedAdminUser()
    {   
        var now = DateTime.UtcNow;
        
        var adminUserTypes = await userTypeRepository.GetTypeByLevelAsync(EAccessLevel.QuinaryLevel);
        
        if (adminUserTypes == null || adminUserTypes.Count == 0 || adminUserTypes[0].Id == Guid.Empty)
        {
            return false;
        }
        
        var existingAdminUser = await userRepository.CheckAvailabilityByFullNameAsync(envInitializer.DefaultAdminUser);
        
        if (existingAdminUser != null)
        {
            return false;
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
            PasswordHash = await passwordService.HashPasswordAsync(envInitializer.DefaultAdminPassword),
            
            CreatedBy = "aspnet-initializer",
            CreatedAt = now,
            UpdatedBy = "aspnet-initializer",
            UpdatedAt = now
        };
        
        if (await userRepository.CreateAsync(adminUser) == null || await userAuthRepository.CreateAsync(adminAuth) == null)
        {
            // TODO: Implement proper error handling
            return false;
        }

        return true;
    }
}
    // TODO: Implement an authentication method that can authenticate soft deleted users (IgnoreQueryFilers)
    
    // TODO: Implement restoration method that cascade-restores UserAuthData, CourseTeachers, Courses, AttendanceChecks