using App.Domain;

namespace App.BLL.Contracts;

public interface IAuthService
{
    string GenerateJwtToken(UserEntity user);
    Task<string?> GenerateRefreshToken(Guid userId, string creatorIp, string creator);
    Task<(string? JwtToken, string? RefreshToken)> RefreshJwtToken(string refreshToken, string jwtToken, string ipAddress, string creator);
    Guid? GetUserIdFromJwt(string jwtToken);
    Task<bool> VerifyRefreshToken(string refreshToken, Guid userId, string ipAddress);
    Task<bool> DeleteRefreshToken(string refreshToken);
    string HashPassword(string input);
    bool VerifyPassword(string input, string storedHash);
}