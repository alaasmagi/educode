using App.Contracts.DTOs;
using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Contracts.WebRequests;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using App.Infrastructure.Initializers;
using Base.Domain;
using Base.DTO;
using Microsoft.Extensions.Logging;

namespace App.Application.Services.User;

public class AuthService(
    IRefreshTokenService refreshTokenService,
    IOtpService otpService,
    IEmailService emailClient,
    IUserTypeRepository userTypeRepository,
    IAccessTokenService accessTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    EnvInitializer envInitializer,
    ILogger<AuthService> logger,
    IUserRepository userRepository,
    IPasswordService passwordService,
    IUserAuthRepository userAuthRepository) : IAuthService
{ 
    public async Task<MethodResponse<(UserDto, string, string)>> AuthenticateUserAsync(LoginRequest request, string clientIp,
                                                                                        string clientApp, bool includeDeleted)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            logger.LogError($"Failed to fetch user with email {request.Email}");
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

        var result = await passwordService.VerifyPasswordAsync(request.Password, userAuthData.PasswordHash);
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
        
        var accessToken = accessTokenService.GenerateAccessToken(user, userAuthData, clientApp);
        var refreshTokenResponse = await refreshTokenService.GenerateRefreshTokenAsync(user.Id, clientIp, request.Email, clientApp);

        if (!refreshTokenResponse.Successful)
        {
            return MethodResponse<(UserDto, string, string)>.Failure(refreshTokenResponse.Error!);
        }
        
        var userDto = new UserDto(user, envInitializer.OciPublicUrl);
        logger.LogInformation($"Successfully authenticated user with ID {user.Id}");
        return MethodResponse<(UserDto, string, string)>.Success((userDto, accessToken, refreshTokenResponse.Value!));
    }

    public async Task<MethodResponse<UserDto>> RegisterUserAsync(CreateAccountRequest request)
    {
        var user = new UserEntity();

        if (await userRepository.CheckAvailabilityByEmailAsync(request.Email) != null)
        {
            logger.LogError($"Failed to create account for user with email {request.Email}");
            return MethodResponse<UserDto>.Failure(
                new Error(ErrorConstants.EmailAlreadyExists, "Email already exists")
            );
        }

        var initialUserTypes = await userTypeRepository.GetTypeByLevelAsync(EAccessLevel.PrimaryLevel);

        if (initialUserTypes == null || initialUserTypes.Count == 0)
        {
            logger.LogError($"Failed to create account for user with email {request.Email}");
            return MethodResponse<UserDto>.Failure(
                new Error(ErrorConstants.UserTypeNotAvailable, "User type is not available")
            );
        }

        user.Email = request.Email;
        user.FullName = request.Fullname;
        user.SchoolId = request.SchoolId;
        user.TypeId = initialUserTypes[0].Id;
        user.StudentCode = request.StudentCode;
        user.CreatedBy = request.Email;
        user.CreatedByClient = request.ClientApp;
        user.UpdatedBy = request.Email;
        user.UpdatedByClient = request.ClientApp;
        
        var passwordHash = await passwordService.HashPasswordAsync(request.Password);
        var userAuthData = new UserAuthEntity
        {
            UserId = user.Id,
            PasswordHash = passwordHash,
            Verified = false,
            CreatedBy = request.Email,
            CreatedByClient = request.ClientApp,
            UpdatedBy = request.Email,
            UpdatedByClient = request.ClientApp
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

    public async Task<MethodResponse<bool>> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var userAuthData = await userAuthRepository.GetByUserAsync(request.UserId);
        if (userAuthData == null)
        {
            logger.LogWarning($"User auth data for user with ID {request.UserId} was not found");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.UserNotFound, "User was not found")
            );
        }
        
        var result = await passwordService.VerifyPasswordAsync(request.CurrentPassword, userAuthData.PasswordHash);
        if (!result)
        {
            logger.LogWarning($"Invalid current password provided for user with ID {request.UserId}");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.InvalidPassword, "Invalid password")
            );
        }
        
        var newPasswordHash = await passwordService.HashPasswordAsync(request.NewPassword);
        userAuthData.PasswordHash = newPasswordHash;
        userAuthData.UpdatedBy = request.Email;
        userAuthData.UpdatedByClient = request.ClientApp;
        userAuthData.Verified = false;
        
        if (await userAuthRepository.UpdateAsync(userAuthData) == null)
        {
            logger.LogError($"Failed to change password for user with ID {request.UserId}");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.PasswordChangeFailed, "Password change failed")
            );
        }
        
        await refreshTokenRepository.RemoveAllByUserAsync(request.UserId);
        
        logger.LogInformation($"Successfully changed password for user with ID {request.UserId}");
        return MethodResponse<bool>.Success(true);
    }

    public async Task<MethodResponse<bool>> LogOutUserAsync(string refreshToken)
    {
        return await refreshTokenService.DeleteRefreshTokenAsync(refreshToken);
    }

    public async Task<MethodResponse<(string AccessToken, string RefreshToken)>> RefreshTokensAsync(string refreshToken, 
                                                                                    string accessToken, string clientApp)
    {
        var email = accessTokenService.GetEmailFromAccessToken(accessToken);
        if (email == null)
        {
            logger.LogError($"Failed to extract email from access token");
            return MethodResponse<(string AccessToken, string RefreshToken)>.Failure(
                new Error(ErrorConstants.EmailNotFound, "Email was not found")
            );
        }
        
        var user = await userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            logger.LogError($"Failed to fetch user with email {email}");
            return MethodResponse<(string AccessToken, string RefreshToken)>.Failure(
                new Error(ErrorConstants.UserNotFound, "User was not found")
            );
        }
        
        var verifyResult = await refreshTokenService.VerifyRefreshTokenAsync(refreshToken, user.Id);
        if (!verifyResult.Successful)
        {
            logger.LogError($"Invalid refresh token for user with ID {user.Id}");
            return MethodResponse<(string AccessToken, string RefreshToken)>.Failure(verifyResult.Error!);
        }
        
        var userAuthData = await userAuthRepository.GetByUserAsync(user.Id);
        if (userAuthData == null)
        {
            logger.LogError($"Failed to fetch user auth data for user with ID {user.Id}");
            return MethodResponse<(string AccessToken, string RefreshToken)>.Failure(
                new Error(ErrorConstants.AuthenticationFailed, "Authentication failed")
            );
        }
        
        var newAccessToken = accessTokenService.GenerateAccessToken(user, userAuthData, clientApp);
        
        await refreshTokenService.DeleteRefreshTokenAsync(refreshToken);
        
        var newRefreshTokenResponse = await refreshTokenService.GenerateRefreshTokenAsync(
            user.Id, "refresh", email, clientApp);
        
        if (!newRefreshTokenResponse.Successful)
        {
            logger.LogError($"Failed to generate new refresh token for user with ID {user.Id}");
            return MethodResponse<(string AccessToken, string RefreshToken)>.Failure(newRefreshTokenResponse.Error!);
        }
        
        logger.LogInformation($"Successfully refreshed tokens for user with ID {user.Id}");
        return MethodResponse<(string AccessToken, string RefreshToken)>.Success((newAccessToken, newRefreshTokenResponse.Value!));
    }

    public async Task<MethodResponse<bool>> GenerateAndSendOtpAsync(OtpRequest request)
    {
        var otpResponse = await otpService.GenerateAndStoreOtp(request.Email);
        if (!otpResponse.Successful)
        {
            return MethodResponse<bool>.Failure(otpResponse.Error!);
        }

        var emailContent = new OtpEmailApiRequest
        {
            EmailTo =  request.Email,
            FullName = request.FullName,
            Otp = otpResponse.Value.ToString(),
            OtpExpirationMinutes = envInitializer.OtpExpirationMinutes
        };
        
        if (!await emailClient.SendOtpAsync(emailContent))
        {
            logger.LogWarning($"User auth data for user with email {request.Email} was not found");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.UserNotFound, "User was not found")
            );
        }
        
        logger.LogInformation($"Successfully generated and sent OTP for user with email {request.Email}");
        return MethodResponse<bool>.Success(true);
    }

    public async Task<MethodResponse<(string AccessToken, string RefreshToken)>> VerifyOtpAsync(VerifyOtpRequest request, 
                                                                                                        string creatorIp)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            logger.LogWarning($"User with email {request.Email} was not found");
            return MethodResponse<(string AccessToken, string RefreshToken)>.Failure(
                new Error(ErrorConstants.UserNotFound, "User was not found")
            );
        }
        
        var otpResponse = await otpService.VerifyOtp(request.Email, request.Otp);
        if (!otpResponse.Successful)
        {
            return MethodResponse<(string AccessToken, string RefreshToken)>.Failure(otpResponse.Error!);
        }
        
        var userAuthData = await userAuthRepository.GetByUserAsync(user.Id);
        if (userAuthData == null)
        {
            logger.LogWarning($"User auth data for user with ID {user.Id} was not found");
            return MethodResponse<(string AccessToken, string RefreshToken)>.Failure(
                new Error(ErrorConstants.UserAuthNotFound, "User auth data was not found")
            );
        }
        
        userAuthData.UpdatedBy = request.Email;
        userAuthData.UpdatedByClient = request.ClientApp;
        userAuthData.Verified = true;
        var updateResponse = await userAuthRepository.UpdateAsync(userAuthData);
        if (updateResponse == null)        {
            logger.LogError($"Failed to verify OTP for user with ID {user.Id}");
            return MethodResponse<(string AccessToken, string RefreshToken)>.Failure(
                new Error(ErrorConstants.OtpVerificationFailed, "OTP verification failed")
            );
        }
        
        var accessToken = accessTokenService.GenerateAccessToken(user, userAuthData, request.ClientApp);
        var refreshToken = await refreshTokenService.GenerateRefreshTokenAsync(user.Id, creatorIp, 
                                                                                        request.Email, request.ClientApp);
        
        if (!refreshToken.Successful || refreshToken.Value == null)
        {
            return MethodResponse<(string AccessToken, string RefreshToken)>.Failure(refreshToken.Error!);
        }

        logger.LogInformation($"Successfully generated and sent OTP for user with ID {user.Id}");
        return MethodResponse<(string AccessToken, string RefreshToken)>.Success((accessToken, refreshToken.Value));
    }
}