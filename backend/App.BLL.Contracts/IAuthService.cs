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
    Task<string> HashPasswordAsync(string input);
    Task<bool> VerifyPasswordAsync(string input, string storedHash);
}