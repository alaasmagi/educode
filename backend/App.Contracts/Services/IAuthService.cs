namespace App.Contracts.Services;

public interface IRefreshTokenService
{
    Task<string?> GenerateRefreshToken(Guid userId, string creatorIp, string creator);
    Task<bool> VerifyRefreshToken(string refreshToken, Guid userId, string ipAddress);
    Task<bool> DeleteRefreshToken(string refreshToken);
}