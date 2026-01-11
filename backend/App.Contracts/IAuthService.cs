using App.Domain;

namespace Contracts;

public interface IAuthService
{
    string? AdminAccessGrant(string enteredUsername, string enteredPassword);
    string GenerateJwtToken(UserEntity? user);
    Task<string?> GenerateRefreshToken(Guid userId, string creatorIp, string creator);
    Task<(string? JwtToken, string? RefreshToken)> RefreshJwtToken(string refreshToken, string jwtToken, string ipAddress, string creator);
    Guid? GetUserIdFromJwt(string jwtToken);
    Task<bool> VerifyRefreshToken(string refreshToken, Guid userId, string ipAddress);
    Task<bool> DeleteRefreshToken(string refreshToken);
}