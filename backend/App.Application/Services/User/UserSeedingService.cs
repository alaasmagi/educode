using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using App.Infrastructure.Initializers;
using Base.Domain;
using Base.DTO;
using Microsoft.Extensions.Logging;

namespace App.Application.Services.User;

public class UserSeedingService(
    IUserRepository userRepository,
    IPasswordService passwordService,
    IUserAuthRepository userAuthRepository,
    IUserTypeRepository userTypeRepository,
    EnvInitializer envInitializer,
    ILogger<UserSeedingService> logger) : IUserSeedingService
{
    public async Task<MethodResponse<bool>> Seed()
    {   
        var adminUserTypes = await userTypeRepository.GetTypeByLevelAsync(EAccessLevel.QuinaryLevel);
        
        if (adminUserTypes == null || adminUserTypes.Count == 0)
        {
            logger.LogWarning("Failed to seed admin user: admin user type was not found");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.AdminUserNotSeeded, "Admin user was not seeded")
            );
        }
        
        var existingAdminUser = await userRepository.CheckAvailabilityByFullNameAsync(envInitializer.DefaultAdminUser);
        
        if (existingAdminUser != null)
        {
            logger.LogWarning("Failed to seed admin user: user with default admin email already exists");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.AdminUserNotSeeded, "Admin user was not seeded")
            );
        }
        
        var adminUser = new UserEntity
        {
            Email = envInitializer.DefaultAdminUser,
            TypeId = adminUserTypes[0].Id,
            FullName = envInitializer.DefaultAdminUser,
            CreatedBy = Constants.BackendName,
            CreatedByClient = Constants.BackendName,
            UpdatedBy = Constants.BackendName,
            UpdatedByClient = Constants.BackendName,
        };
        
        var adminAuth = new UserAuthEntity
        {
            UserId = adminUser.Id,
            PasswordHash = await passwordService.HashPasswordAsync(envInitializer.DefaultAdminPassword),
            Verified = true,
            CreatedBy = Constants.BackendName,
            CreatedByClient = Constants.BackendName,
            UpdatedBy = Constants.BackendName,
            UpdatedByClient = Constants.BackendName,
        };
        
        if (await userRepository.CreateAsync(adminUser) == null || await userAuthRepository.CreateAsync(adminAuth) == null)
        {
            logger.LogError("Failed to seed admin user");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.AdminUserNotSeeded, "Admin user was not seeded")
            );
        }
        
        logger.LogInformation("Successfully seeded admin user with default email and password");
        return MethodResponse<bool>.Success(true);
    }
}

