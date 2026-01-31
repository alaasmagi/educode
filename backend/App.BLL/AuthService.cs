using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using App.BLL.Contracts;
using App.Common;
using App.DAL.Contracts;
using App.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace App.BLL;

public class AuthService (
    EnvInitializer envInitializer, 
    ICacheRepository cacheRepository, 
    IRefreshTokenRepository refreshTokenRepository, 
    IUserRepository userRepository,
    ILogger<AuthService> logger) : IAuthService
{
    public string GenerateJwtToken(UserEntity user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtKey = envInitializer.JwtKey;
        var issuer = envInitializer.JwtIssuer;
        var audience = envInitializer.JwtAudience;
        
        var jwtExpirationMinutes = envInitializer.JwtExpirationMinutes;
        var now = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            logger.LogError("Reading data from env failed (JWTKEY)");
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
        {
            logger.LogError("Reading data from env failed (JWTISS or JWTAUD)");
            return string.Empty;
        }

        var key = Encoding.ASCII.GetBytes(jwtKey);

        List<Claim> claims = [
            new Claim(Constants.UserIdClaim, user.Id.ToString()),
            new Claim(Constants.AccessLevelClaim,
                ((int)(user.Type?.AccessLevel ?? EAccessLevel.NoAccess)).ToString())
        ];
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = now.AddMinutes(jwtExpirationMinutes),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
    
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
    
    public async Task<(string? JwtToken, string? RefreshToken)> RefreshJwtToken(string refreshToken, string jwtToken, 
                                                                                        string ipAddress, string creator)
    {
        var userId = GetUserIdFromJwt(jwtToken);
        if (userId == null)
        {
            logger.LogError("JWT validation failed");
            return (null, null);
        }

        if (!await VerifyRefreshToken(refreshToken, userId.Value, ipAddress))
        {
            logger.LogError($"Refresh token validation for user with ID {userId} failed");
            return (null, null);
        }

        var user = await userRepository.GetByIdAsync(userId.Value);
        var newJwtToken = GenerateJwtToken(user!);
        
        await cacheRepository.DeletePatternAsync($"*{refreshToken}*");
        var newRefreshToken = await GenerateRefreshToken(userId.Value, ipAddress, creator);
        
        logger.LogInformation($"JWT and refresh tokens successfully refreshed for user with ID: {userId}");
        return (newJwtToken, newRefreshToken);
    }
    
    public Guid? GetUserIdFromJwt(string jwtToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwtToken);
        var userIdClaim = token.Claims.FirstOrDefault(c => c.Type == Constants.UserIdClaim);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        
        return null;
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
            tokenEntity = await refreshTokenRepository.GetByItself(refreshToken);
            
            if (tokenEntity == null)
            {
                return false;
            }
            
            var json = JsonSerializer.Serialize(tokenEntity);
            await cacheRepository.SetAsync(Constants.RefreshTokenPrefix + tokenEntity.Token, json, Constants.DefaultCachePeriod);
        }

        if (tokenEntity.UserId != userId || tokenEntity.Token != refreshToken || tokenEntity.ClientIp != ipAddress)
        {
            return false;
        }
        
        return true;
    }

    public async Task<bool> DeleteRefreshToken(string refreshToken)
    {
        var token = await refreshTokenRepository.GetByItself(refreshToken);
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