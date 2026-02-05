using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Domain.Entities;
using App.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace App.Application.Services;

public class AuthService(
    IRefreshTokenService refreshTokenService,
    IOtpService otpService,
    IUserTypeRepository userTypeRepository,
    IAccessTokenService accessTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    ILogger<AuthService> logger,
    IUserRepository userRepository,
    IPasswordService passwordService,
    IUserAuthRepository userAuthRepository) : IAuthService
{ 
    public async Task<(UserEntity, string, string)?> AuthenticateUserAsync(string email, string password, string clientIp,
                                                                                        string client, bool includeDeleted)
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

        var result = await passwordService.VerifyPasswordAsync(password, userAuthData.PasswordHash);
        if (!result)
        {
            logger.LogError($"Failed to authenticate user with ID {user.Id}");
            return null;
        }
        
        if (!userAuthData.Verified)
        {
            logger.LogError($"User with ID {user.Id} is not verified");
            return null;
        }
        
        var jwtToken = accessTokenService.GenerateAccessToken(user, userAuthData);
        var refreshToken = await refreshTokenService.GenerateRefreshToken(user.Id, clientIp, client);

        if (refreshToken == null)
        {
            return null;
        }
        
        return (user, jwtToken, refreshToken);
    }

    public async Task<UserEntity?> RegisterUserAsync(string email, string fullName, string password, Guid schoolId, 
                                                                                    string client, string? studentCode)
    {
        var user = new UserEntity();

        if (await userRepository.CheckAvailabilityByEmailAsync(email) != null)
        {
            logger.LogError($"Failed to create account for user with email {user.Email}");
            return null;
        }

        var initialUserTypes = await userTypeRepository.GetTypeByLevelAsync(EAccessLevel.PrimaryLevel);

        if (initialUserTypes == null || initialUserTypes.Count == 0)
        {
            logger.LogError($"Failed to create account for user with email {user.Email}");
            return null;
        }

        user.Email = email;
        user.FullName = fullName;
        user.SchoolId = schoolId;
        user.TypeId = initialUserTypes[0].Id;
        user.StudentCode = studentCode;
        user.CreatedBy = client;
        user.UpdatedBy = client;
        
        var passwordHash = await passwordService.HashPasswordAsync(password);
        var userAuthData = new UserAuthEntity
        {
            UserId = user.Id,
            PasswordHash = passwordHash,
            Verified = false,
            CreatedBy = client,
            UpdatedBy = client
        };
        
        if (await userRepository.CreateAsync(user) == null)
        {
            logger.LogError($"Failed to create account for user with email {user.Email}");
            return null;
        }
        
        if (await userAuthRepository.CreateAsync(userAuthData) == null)
        {
            logger.LogError($"Failed to create account for user with email {user.Email}");
            return null;
        }

        return user;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId , string currentPassword, string newPassword, string client)
    {
        var userAuthData = await userAuthRepository.GetByUserAsync(userId);
        if (userAuthData == null)
        {
            logger.LogError($"Failed to fetch user auth data for user with ID {userId}");
            return false;
        }
        
        var result = await passwordService.VerifyPasswordAsync(currentPassword, userAuthData.PasswordHash);
        if (!result)
        {
            return false;
        }
        
        var newPasswordHash = await passwordService.HashPasswordAsync(newPassword);
        userAuthData.PasswordHash = newPasswordHash;
        userAuthData.UpdatedBy = client;
        
        if (await userAuthRepository.UpdateAsync(userAuthData) == null)
        {
            logger.LogError($"Failed to change password for user with ID {userId}");
            return false;
        }
        
        await refreshTokenRepository.RemoveAllByUserAsync(userId);
        
        return true;
    }

    public async Task<bool> LogOutUserAsync(string refreshToken)
    {
        return await refreshTokenService.DeleteRefreshToken(refreshToken);
    }

    public Task<(string AccessToken, string RefreshToken)?> RefreshTokensAsync(string refreshToken, string accessToken, string client)
    {
        throw new NotImplementedException();
    }
}