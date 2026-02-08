using System.Text.Json;
using App.Contracts.DTOs;
using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Domain.Entities;
using App.Infrastructure.Helpers;
using App.Infrastructure.Initializers;
using Base.DTO;
using Microsoft.Extensions.Logging;

namespace App.Application.Services.User;

public class UserService(
    IUserRepository userRepository,
    IUserTypeRepository userTypeRepository,
    ICourseTeacherRepository courseTeacherRepository,
    IAttendanceCheckRepository attendanceCheckRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUserAuthRepository userAuthRepository,
    ICacheRepository cacheRepository,
    EnvInitializer envInitializer,
    ILogger<UserService> logger) : IUserService
{
    public async Task<MethodResponse<List<UserDto>>> GetAllUsersAsync(int pageNr, int pageSize)
    {
        var cache = await cacheRepository.GetAsync(Constants.UserPrefix + pageNr + pageSize);
        if (cache != null)
        {
            var deserializedUsers = JsonSerializer.Deserialize<List<UserDto>?>(cache);
            return MethodResponse<List<UserDto>>.Success(deserializedUsers!);
        }

        var result = await userRepository.GetAllAsync(pageNr, pageSize);

        if (result == null)
        {
            logger.LogWarning("Failed to get all users");
            return MethodResponse<List<UserDto>>.Failure(
                new Error(ErrorConstants.UsersNotFound, "Users were not found")
            );
        }

        var userDtos = UserDto.ToDtoList(result, envInitializer.OciPublicUrl);
        var serializedUserDtos = JsonSerializer.Serialize(userDtos);
        await cacheRepository.SetAsync(
            Constants.UserPrefix + pageNr + pageSize,
            serializedUserDtos, Constants.ShortCachePeriod);

        return MethodResponse<List<UserDto>>.Success(userDtos);
    }
    
    public async Task<MethodResponse<UserDto>> GetUserByIdAsync(Guid id)
    {
        var cache = await cacheRepository.GetAsync(Constants.UserPrefix + id);
        if (cache != null)
        {
            var deserializedUser = JsonSerializer.Deserialize<UserDto?>(cache);
            return MethodResponse<UserDto>.Success(deserializedUser!);
        }

        var result = await userRepository.GetByIdAsync(id);

        if (result == null)
        {
            logger.LogWarning($"User with ID {id} not found");
            return MethodResponse<UserDto>.Failure(
                new Error(ErrorConstants.UserNotFound, "User was not found")
            );
        }

        var userDto = new UserDto(result, envInitializer.OciPublicUrl);
        var serializedUserDto = JsonSerializer.Serialize(userDto);
        await cacheRepository.SetAsync(Constants.UserPrefix + id,
            serializedUserDto, Constants.DefaultCachePeriod);

        return MethodResponse<UserDto>.Success(userDto);
    }
    
    public async Task<MethodResponse<bool>> UpdateUserAsync(UserEntity user)
    {
        await cacheRepository.DeletePatternAsync($"*{user.Id.ToString()}*");
        await cacheRepository.DeletePatternAsync($"*{user.Email}*");

        var status = await userRepository.UpdateAsync(user);
        if (status == null)
        {
            logger.LogWarning($"Failed to update user with ID {user.Id}");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.UserNotUpdated, "User was not updated")
            );
        }

        logger.LogInformation($"Successfully updated user with ID {user.Id}");
        return MethodResponse<bool>.Success(true);
    }
    
    public async Task<MethodResponse<bool>> SoftDeleteUserAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            logger.LogWarning($"User with ID {userId} was not soft-deleted");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.UserNotDeleted, "User was not soft-deleted")
            );
        }

        await cacheRepository.DeletePatternAsync($"*{userId.ToString()}*");
        await cacheRepository.DeletePatternAsync($"*{user.Email}*");

        await courseTeacherRepository.ToggleDeletionForAllByTeacherAsync(userId, true);
        await attendanceCheckRepository.ToggleDeletionForAllByUserAsync(user.FullName, true);
        await refreshTokenRepository.RemoveAllByUserAsync(userId);

        var userAuth = await userAuthRepository.GetByUserAsync(userId);
        if (await userRepository.ToggleDeletionAsync(userId, true) ||
            await userAuthRepository.ToggleDeletionAsync(userAuth!.Id, false))
        {
            logger.LogWarning($"Failed to delete user with ID {userId}");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.UserNotDeleted, "User was not deleted")
            );
        }

        logger.LogInformation($"Successfully deleted user with ID {userId}");
        return MethodResponse<bool>.Success(true);
    }
    
    public async Task<MethodResponse<bool>> RestoreUserAsync(Guid userId)
    {
        await userRepository.ToggleDeletionAsync(userId, false);
        var user = await userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            logger.LogWarning($"User with ID {userId} was not restored");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.UserNotRestored, "User was not deleted")
            );
        }

        await courseTeacherRepository.ToggleDeletionForAllByTeacherAsync(userId, false);
        await attendanceCheckRepository.ToggleDeletionForAllByUserAsync(user.FullName, false);

        var userAuth = await userAuthRepository.GetByUserAsync(userId);
        if (await userRepository.ToggleDeletionAsync(userId, false) ||
            await userAuthRepository.ToggleDeletionAsync(userAuth!.Id, false))
        {
            logger.LogError($"Failed to restore user with ID {userId}");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.UserNotRestored, "User was not restored")
            );
        }

        logger.LogInformation($"Successfully restored user with ID {userId}");
        return MethodResponse<bool>.Success(true);
    }
    
    public async Task<MethodResponse<UserTypeEntity>> GetUserTypeAsync(string userType)
    {
        var cache = await cacheRepository.GetAsync(Constants.UserTypePrefix + userType);
        if (cache != null)
        {
            var deserializedUserType = JsonSerializer.Deserialize<UserTypeEntity?>(cache);
            return MethodResponse<UserTypeEntity>.Success(deserializedUserType!);
        }
        
        var result = await userTypeRepository.GetByItselfAsync(userType);
        if (result == null)
        {
            logger.LogWarning($"Failed to get user type {userType}");
            return MethodResponse<UserTypeEntity>.Failure(
                new Error(ErrorConstants.UserTypeNotFound, "User type was not found")
            );
        }
        
        var serializedUserType = JsonSerializer.Serialize(result);
        await cacheRepository.SetAsync(Constants.UserTypePrefix + userType, 
            serializedUserType, Constants.ExtraLongCachePeriod);
        
        logger.LogInformation($"Successfully retrieved user type {userType}");
        return MethodResponse<UserTypeEntity>.Success(result);
    }
}

