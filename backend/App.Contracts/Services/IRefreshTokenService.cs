using Base.DTO;

namespace App.Contracts.Services;

public interface IRefreshTokenService
{
    Task<MethodResponse<string>> GenerateRefreshToken(Guid userId, string creatorIp, string client);
    Task<MethodResponse<bool>> VerifyRefreshToken(string refreshToken, Guid userId);
    Task<MethodResponse<bool>> DeleteRefreshToken(string refreshToken);
}