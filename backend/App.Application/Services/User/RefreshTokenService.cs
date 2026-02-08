using System.Security.Cryptography;
using System.Text.Json;
using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Domain.Entities;
using App.Infrastructure.Helpers;
using App.Infrastructure.Initializers;
using Base.Domain;
using Base.DTO;
using Microsoft.Extensions.Logging;

namespace App.Application.Services.User;

public class RefreshTokenService (
    EnvInitializer envInitializer,
    ICacheRepository cacheRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ILogger<RefreshTokenService> logger) : IRefreshTokenService
{
    public async Task<MethodResponse<string>> GenerateRefreshTokenAsync(Guid userId, string creatorIp, string email, string clientApp)
    {
        var refreshTokenExpirationDays = envInitializer.RefreshTokenExpirationDays;
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        
        var tokenData = new RefreshTokenEntity
        {
            UserId = userId,
            Token = token,
            Client = email,
            ClientIp = creatorIp,
            ExpirationTime = DateTime.UtcNow + TimeSpan.FromDays(refreshTokenExpirationDays),
            CreatedBy = email,
            CreatedByClient = clientApp,
            UpdatedBy = email,
            UpdatedByClient = clientApp
        };
        
        if (await refreshTokenRepository.CreateAsync(tokenData) == null)
        {
            logger.LogError($"Refresh token creation failed for user with ID: {userId}");
            return MethodResponse<string>.Failure(
                new Error(ErrorConstants.RefreshTokenNotGenerated, "Refresh token generation failed")
            );
        }
        
        var json = JsonSerializer.Serialize(tokenData);
        await cacheRepository.SetAsync(Constants.RefreshTokenPrefix + token, json, Constants.DefaultCachePeriod);
        logger.LogInformation($"Refresh token creation successfully for user with ID: {userId}");
        return MethodResponse<string>.Success(token);
    }
    
    public async Task<MethodResponse<bool>> VerifyRefreshTokenAsync(string refreshToken, Guid userId)
    {
        var cache = await cacheRepository.GetAsync(Constants.RefreshTokenPrefix + refreshToken);
        
        RefreshTokenEntity? tokenEntity;
        if (cache != null)
        {
            tokenEntity = JsonSerializer.Deserialize<RefreshTokenEntity>(cache);
            
            if (tokenEntity == null)
            {
                return MethodResponse<bool>.Failure(
                    new Error(ErrorConstants.RefreshTokenNotFound, "Refresh token not found")
                );
            }
        }
        else
        {
            tokenEntity = await refreshTokenRepository.GetByItselfAsync(refreshToken);
            
            if (tokenEntity == null)
            {
                return MethodResponse<bool>.Failure(
                    new Error(ErrorConstants.RefreshTokenNotFound, "Refresh token not found")
                );
            }
            
            var json = JsonSerializer.Serialize(tokenEntity);
            await cacheRepository.SetAsync(Constants.RefreshTokenPrefix + tokenEntity.Token, json, Constants.DefaultCachePeriod);
        }

        if (tokenEntity.UserId != userId || tokenEntity.Token != refreshToken)
        {
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.RefreshTokenNotVerified, "Refresh token verification failed")
            );
        }
        
        return MethodResponse<bool>.Success(true);
    }

    public async Task<MethodResponse<bool>> DeleteRefreshTokenAsync(string refreshToken)
    {
        var token = await refreshTokenRepository.GetByItselfAsync(refreshToken);
        if (token == null || await refreshTokenRepository.RemoveAsync(token) == null)
        {
            logger.LogError("Failed to delete refresh token");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.RefreshTokenNotDeleted, "Refresh token deletion failed")
            );
        }

        await cacheRepository.DeletePatternAsync($"*{refreshToken}*");
        logger.LogInformation("Successfully deleted refresh token");
        return MethodResponse<bool>.Success(true);
    }
}