using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using App.Contracts.DTOs;
using App.Contracts.Services;
using App.Domain.Entities;
using App.Domain.Enums;
using App.Infrastructure.Helpers;
using App.Infrastructure.Initializers;
using Microsoft.IdentityModel.Tokens;

namespace App.Infrastructure.JWT;

public class JwtService(
    EnvInitializer envInitializer) : IAccessTokenService
{
    public string GenerateAccessToken(UserEntity user, UserAuthEntity userAuth)
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
            new Claim(Constants.VerificationClaim, userAuth.Verified.ToString()),
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
    
    public string GenerateAdminAccessToken(UserDto user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtKey = envInitializer.JwtKey;
        var issuer = envInitializer.JwtIssuer;
        var audience = envInitializer.JwtAudience;
        
        var jwtExpirationMinutes = envInitializer.JwtAdminExpirationMinutes;
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
            new Claim(Constants.AccessLevelClaim, EAccessLevel.QuinaryLevel.ToString()),
            new Claim(Constants.VerificationClaim, true.ToString()),
            new Claim(Constants.SchoolIdClaim, string.Empty)
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