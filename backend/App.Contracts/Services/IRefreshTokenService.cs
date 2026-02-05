namespace App.Contracts.Services;

public interface IRefreshTokenService
{
    Task<string?> GenerateRefreshToken(Guid userId, string creatorIp, string client);
    Task<bool> VerifyRefreshToken(string refreshToken, Guid userId);
    Task<bool> DeleteRefreshToken(string refreshToken);
}