using App.Contracts.DTOs;
using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using App.Infrastructure.Initializers;
using Base.DTO;
using Microsoft.Extensions.Logging;

namespace App.Application.Services.User;

public class AuthService(
    IRefreshTokenService refreshTokenService,
    IUserTypeRepository userTypeRepository,
    IAccessTokenService accessTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    EnvInitializer envInitializer,
    ILogger<AuthService> logger,
    IUserRepository userRepository,
    IPasswordService passwordService,
    IUserAuthRepository userAuthRepository) : IAuthService
{ 
    public async Task<MethodResponse<(UserDto, string, string)>> AuthenticateUserAsync(string email, string password, string clientIp,
                                                                                        string client, bool includeDeleted)
    {
        var user = await userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            logger.LogError($"Failed to fetch user with email {email}");
            return MethodResponse<(UserDto, string, string)>.Failure(
                new Error(ErrorConstants.UserNotFound, "User was not found")
            );
        }
        
        var userAuthData = await userAuthRepository.GetByUserAsync(user.Id);
        if (userAuthData == null)
        {
            logger.LogError($"Failed to fetch user auth data for user with ID {user.Id}");
            return MethodResponse<(UserDto, string, string)>.Failure(
                new Error(ErrorConstants.AuthenticationFailed, "Authentication failed")
            );
        }

        var result = await passwordService.VerifyPasswordAsync(password, userAuthData.PasswordHash);
        if (!result)
        {
            logger.LogError($"Failed to authenticate user with ID {user.Id}");
            return MethodResponse<(UserDto, string, string)>.Failure(
                new Error(ErrorConstants.InvalidPassword, "Invalid password")
            );
        }
        
        if (!userAuthData.Verified)
        {
            logger.LogError($"User with ID {user.Id} is not verified");
            return MethodResponse<(UserDto, string, string)>.Failure(
                new Error(ErrorConstants.UserNotVerified, "User is not verified")
            );
        }
        
        var jwtToken = accessTokenService.GenerateAccessToken(user, userAuthData);
        var refreshTokenResponse = await refreshTokenService.GenerateRefreshToken(user.Id, clientIp, client);

        if (!refreshTokenResponse.Successful)
        {
            return MethodResponse<(UserDto, string, string)>.Failure(refreshTokenResponse.Error!);
        }
        
        var userDto = new UserDto(user, envInitializer.OciPublicUrl);
        logger.LogInformation($"Successfully authenticated user with ID {user.Id}");
        return MethodResponse<(UserDto, string, string)>.Success((userDto, jwtToken, refreshTokenResponse.Value!));
    }

    public async Task<MethodResponse<UserDto>> RegisterUserAsync(string email, string fullName, string password, Guid schoolId, 
                                                                                    string client, string? studentCode)
    {
        var user = new UserEntity();

        if (await userRepository.CheckAvailabilityByEmailAsync(email) != null)
        {
            logger.LogError($"Failed to create account for user with email {email}");
            return MethodResponse<UserDto>.Failure(
                new Error(ErrorConstants.EmailAlreadyExists, "Email already exists")
            );
        }

        var initialUserTypes = await userTypeRepository.GetTypeByLevelAsync(EAccessLevel.PrimaryLevel);

        if (initialUserTypes == null || initialUserTypes.Count == 0)
        {
            logger.LogError($"Failed to create account for user with email {email}");
            return MethodResponse<UserDto>.Failure(
                new Error(ErrorConstants.UserTypeNotAvailable, "User type is not available")
            );
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
            return MethodResponse<UserDto>.Failure(
                new Error(ErrorConstants.RegistrationFailed, "User registration failed")
            );
        }
        
        if (await userAuthRepository.CreateAsync(userAuthData) == null)
        {
            logger.LogError($"Failed to create account for user with email {user.Email}");
            return MethodResponse<UserDto>.Failure(
                new Error(ErrorConstants.RegistrationFailed, "User registration failed")
            );
        }
        
        var userDto = new UserDto(user, envInitializer.OciPublicUrl);
        logger.LogInformation($"Successfully created user with ID {user.Id}");
        return MethodResponse<UserDto>.Success(userDto);
    }

    public async Task<MethodResponse<bool>> ChangePasswordAsync(Guid userId , string currentPassword, string newPassword, string client)
    {
        var userAuthData = await userAuthRepository.GetByUserAsync(userId);
        if (userAuthData == null)
        {
            logger.LogWarning($"User auth data for user with ID {userId} was not found");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.UserNotFound, "User was not found")
            );
        }
        
        var result = await passwordService.VerifyPasswordAsync(currentPassword, userAuthData.PasswordHash);
        if (!result)
        {
            logger.LogWarning($"Invalid current password provided for user with ID {userId}");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.InvalidPassword, "Invalid password")
            );
        }
        
        var newPasswordHash = await passwordService.HashPasswordAsync(newPassword);
        userAuthData.PasswordHash = newPasswordHash;
        userAuthData.UpdatedBy = client;
        
        if (await userAuthRepository.UpdateAsync(userAuthData) == null)
        {
            logger.LogError($"Failed to change password for user with ID {userId}");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.PasswordChangeFailed, "Password change failed")
            );
        }
        
        await refreshTokenRepository.RemoveAllByUserAsync(userId);
        
        logger.LogInformation($"Successfully changed password for user with ID {userId}");
        return MethodResponse<bool>.Success(true);
    }

    public async Task<MethodResponse<bool>> LogOutUserAsync(string refreshToken)
    {
        return await refreshTokenService.DeleteRefreshToken(refreshToken);
    }

    public Task<MethodResponse<(string AccessToken, string RefreshToken)>> RefreshTokensAsync(string refreshToken, string accessToken, string client)
    {
        throw new NotImplementedException();
    }
}