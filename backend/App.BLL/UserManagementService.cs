using System.Text.Json;
using App.DAL.EF;
using App.Domain;
using Contracts;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace App.BLL;

public class UserManagementService : IUserManagementService
{
    private readonly UserRepository _userRepository;
    private readonly CourseRepository _courseRepository;
    private readonly RedisRepository _redisRepository;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(AppDbContext context, ILogger<UserManagementService> logger, 
                                    IConnectionMultiplexer connectionMultiplexer, ILogger<RedisRepository> redisLogger)
    {
        _userRepository = new UserRepository(context); 
        _courseRepository = new CourseRepository(context); 
        _redisRepository = new RedisRepository(connectionMultiplexer, redisLogger); 
        _logger = logger;
    }
    
    public async Task<UserEntity?> AuthenticateUserAsync(Guid userId, string password)
    {
        var userAuthData = await _userRepository.GetUserAuthDataByUserId(userId);

        if (userAuthData == null)
        {
            _logger.LogError($"Failed to fetch user auth data for user with ID {userId}");
            return null;
        }

        var result = VerifyPasswordHash(password, userAuthData.PasswordHash);
        
        if (!result)
        {
            _logger.LogError($"Failed to authenticate user with ID {userId}");
            return null;
        }
        
        return userAuthData.User;
    }

    public async Task<bool> CreateAccountAsync(UserEntity user, UserAuthEntity userAuthData)
    {
        if (await DoesUserExistAsync(user.Email))
        {
            _logger.LogError($"Failed to create account for user with email {user.Email}");
            return false;
        }

        if (user.StudentCode != null)
        {
            user.StudentCode = user.StudentCode.ToUpper();    
        }
        
        if (!await _userRepository.AddUserEntityToDb(user))
        {
            _logger.LogError($"Failed to create account for user with email {user.Email}");
            return false;
        }
        
        userAuthData.UserId = user.Id;
        if (!await _userRepository.AddUserAuthEntityToDb(userAuthData))
        {
            _logger.LogError($"Failed to create account for user with email {user.Email}");
            return false;
        }

        return true;
    }

    public async Task<bool> ChangeUserPasswordAsync(UserEntity user, string newPasswordHash)
    { 
        var status = await _userRepository.UpdateUserAuthEntity(user.Id, newPasswordHash);
        
        if (!status)
        {
            _logger.LogError($"Failed to change user password for user with ID {user.Id}");
            return false;
        }

        return true;
    }
    
    private static bool VerifyPasswordHash(string enteredPassword, string storedHash)
    {
        return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
    }

    public async Task<bool> DoesUserExistAsync(string email)
    {
        var status = await _userRepository.UserAvailabilityCheckByEmail(email);
        
        if (!status)
        {
            _logger.LogError($"User with email {email} was not found");
            return false;
        }
        
        _logger.LogInformation($"User with email {email} was found");
        return true;
    }
    
    public async Task<UserTypeEntity?> GetUserTypeAsync(string userType)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.UserTypePrefix + userType);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<UserTypeEntity?>(cache);
        }
        
        var result = await _userRepository.GetUserTypeEntity(userType);
        if (result == null)
        {
            _logger.LogError($"Failed to get user type {userType}");
            return null;
        }
        
        var serializedUserType = JsonSerializer.Serialize(result);
        await _redisRepository.SetDataAsync(Constants.UserTypePrefix + userType, 
            serializedUserType, Constants.ExtraLongCachePeriod);
        
        return  result;
    }

    public async Task<List<UserEntity>?> GetAllUsersAsync(int pageNr, int pageSize)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.UserPrefix + pageNr + pageSize);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<List<UserEntity>?>(cache);
        }

        var result = await _userRepository.GetAllUsersAsync(pageNr, pageSize);

        if (result.Count <= 0)
        {
            _logger.LogError("Failed to get all users");
            return null;
        }

        var serializedUsers = JsonSerializer.Serialize(result);
        await _redisRepository.SetDataAsync(
            Constants.UserPrefix + pageNr + pageSize,
            serializedUsers, Constants.ShortCachePeriod);

        return result;
        
    }

    public async Task<UserEntity?> GetUserByEmailAsync(string email)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.UserPrefix + email);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<UserEntity?>(cache);
        }
        
        var result = await _userRepository.GetUserByEmailAsync(email);
        
        if (result == null)
        {
            _logger.LogError($"User with email {email} not found");
            return null;
        }
        
        var serializedUser = JsonSerializer.Serialize(result);
        await _redisRepository.SetDataAsync(Constants.UserPrefix + email,
            serializedUser, Constants.DefaultCachePeriod);

        return result;
    }
    
    public async Task<UserEntity?> GetUserByIdAsync(Guid id)
    {
        var cache = await _redisRepository.GetDataAsync(Constants.UserPrefix + id);
        if (cache != null)
        {
            return JsonSerializer.Deserialize<UserEntity?>(cache);
        }
        
        var result = await _userRepository.GetUserByIdAsync(id);

        if (result == null)
        {
            _logger.LogError($"User with ID {id} not found");
            return null;
        }
        
        var serializedUser = JsonSerializer.Serialize(result);
        await _redisRepository.SetDataAsync(Constants.UserPrefix + id,
            serializedUser, Constants.DefaultCachePeriod);

        return result;
    }
    
    public string GetPasswordHash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }
    
    public async Task<bool> DeleteUserAsync(UserEntity user)
    {
        await _redisRepository.DeleteKeysByPatternAsync($"*{user.Id.ToString()}*");
        await _redisRepository.DeleteKeysByPatternAsync($"*{user.Email}*");
        await _redisRepository.DeleteKeysByPatternAsync($"*{user.Id}*");
        bool status = await _userRepository.DeleteUserEntity(user);
        if (!status)
        {
            _logger.LogError($"Failed to delete user with ID {user.Id}");
            return false;
        }

        _logger.LogInformation($"Successfully deleted user with ID {user.Id}");
        return true;
    }
    
    // TODO: Implement Edit User method
    
    public async Task<bool> UpdateUserAsync(UserEntity user)
    {
        await _redisRepository.DeleteKeysByPatternAsync($"*{user.Id.ToString()}*");
        await _redisRepository.DeleteKeysByPatternAsync($"*{user.Email}*");
        
        bool status = await _userRepository.UpdateUserEntity(user);
        if (!status)
        {
            _logger.LogError($"Failed to update user with ID {user.Id}");
            return false;
        }

        _logger.LogInformation($"Successfully updated user with ID {user.Id}");
        return true;
    }
    
    /* TODO: Implement soft deletion that cascade-soft-deletes UserAuthData, CourseTeachers, Courses, AttendanceChecks
                and HARD-deletes all User's RefreshTokens */

    
    // TODO: Implement an authentication method that can authenticate soft deleted users (IgnoreQueryFilers)
    
    // TODO: Implement restoration method that cascade-restores UserAuthData, CourseTeachers, Courses, AttendanceChecks

}