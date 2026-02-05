using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using App.Application;
using App.Application.Initializers;
using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace App.Infrastructure.JWT;

public class JwtService(
    EnvInitializer envInitializer,
    ICacheRepository cacheRepository,
    IUserRepository userRepository) : IAccessTokenService
{
    public string GenerateAccessToken(UserEntity user, UserAuthEntity? userAuth)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtKey = envInitializer.JwtKey;
        var issuer = envInitializer.JwtIssuer;
        var audience = envInitializer.JwtAudience;
        
        var jwtExpirationMinutes = envInitializer.JwtExpirationMinutes;
        var now = DateTime.UtcNow;

        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            return string.Empty;
        }

        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
        {
            return string.Empty;
        }

        var key = Encoding.ASCII.GetBytes(jwtKey);

        List<Claim> claims = [
            new Claim(Constants.UserIdClaim, user.Id.ToString()),
            new Claim(Constants.AccessLevelClaim,
                ((int)(user.Type?.AccessLevel ?? EAccessLevel.NoAccess)).ToString()),
            new Claim(Constants.VerificationClaim, (userAuth?.Verified ?? false).ToString()),
            new Claim(Constants.SchoolIdClaim, user.SchoolId.ToString() ?? string.Empty)
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
    
    private Guid? GetUserIdFromJwt(string jwtToken)
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
}