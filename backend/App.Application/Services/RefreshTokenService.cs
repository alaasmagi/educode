using System.Security.Cryptography;
using System.Text.Json;
using App.Application.Initializers;
using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Domain.Entities;
using App.Infrastructure.Helpers;
using Microsoft.Extensions.Logging;
namespace App.Application.Services;

public class RefreshTokenService (
    EnvInitializer envInitializer,
    ICacheRepository cacheRepository,
    IRefreshTokenRepository refreshTokenRepository,
    ILogger<RefreshTokenService> logger) : IRefreshTokenService
{
    public async Task<string?> GenerateRefreshToken(Guid userId, string creatorIp, string creator)
    {
        var refreshTokenExpirationDays = envInitializer.RefreshTokenExpirationDays;
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        
        var tokenData = new RefreshTokenEntity
        {
            UserId = userId,
            Token = token,
            Client = creator,
            ClientIp = creatorIp,
            ExpirationTime = DateTime.UtcNow + TimeSpan.FromDays(refreshTokenExpirationDays),
            CreatedBy = Constants.BackendPrefix,
            UpdatedBy = Constants.BackendPrefix
        };
        
        if (await refreshTokenRepository.CreateAsync(tokenData) == null)
        {
            logger.LogError($"Refresh token creation failed for user with ID: {userId}");
            return null;
        }
        
        var json = JsonSerializer.Serialize(tokenData);
        await cacheRepository.SetAsync(Constants.RefreshTokenPrefix + token, json, Constants.DefaultCachePeriod);
        logger.LogInformation($"Refresh token creation successfully for user with ID: {userId}");
        return token;
    }
    
    public async Task<bool> VerifyRefreshToken(string refreshToken, Guid userId, string ipAddress)
    {
        var cache = await cacheRepository.GetAsync(Constants.RefreshTokenPrefix + refreshToken);
        
        RefreshTokenEntity? tokenEntity;
        if (cache != null)
        {
            tokenEntity = JsonSerializer.Deserialize<RefreshTokenEntity>(cache);
            
            if (tokenEntity == null)
            {
                return false;
            }
        }
        else
        {
            tokenEntity = await refreshTokenRepository.GetByItselfAsync(refreshToken);
            
            if (tokenEntity == null)
            {
                return false;
            }
            
            var json = JsonSerializer.Serialize(tokenEntity);
            await cacheRepository.SetAsync(Constants.RefreshTokenPrefix + tokenEntity.Token, json, Constants.DefaultCachePeriod);
        }

        if (tokenEntity.UserId != userId || tokenEntity.Token != refreshToken)
        {
            return false;
        }
        
        return true;
    }

    public async Task<bool> DeleteRefreshToken(string refreshToken)
    {
        var token = await refreshTokenRepository.GetByItselfAsync(refreshToken);
        if (token == null || await refreshTokenRepository.RemoveAsync(token) == null)
        {
            logger.LogError($"Refresh token deletion failed");
            return false;
        }

        await cacheRepository.DeletePatternAsync($"*{refreshToken}*");
        logger.LogInformation($"Refresh token deletion successfully");
        return true;
    }
}