using App.Domain.Entities;

namespace App.Contracts.Services;

public interface ITokenService
{
    string GenerateAccessToken(UserEntity user);
    Task<(string? AccessToken, string? RefreshToken)> RefreshTokensAsync(
        string refreshToken, 
        string accessToken, 
        string ipAddress, 
        string creator);
}