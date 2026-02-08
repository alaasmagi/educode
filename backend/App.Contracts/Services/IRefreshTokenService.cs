using Base.DTO;

namespace App.Contracts.Services;

public interface IRefreshTokenService
{
    Task<MethodResponse<string>> GenerateRefreshTokenAsync(Guid userId, string creatorIp, string email, string clientApp);
    Task<MethodResponse<bool>> VerifyRefreshTokenAsync(string refreshToken, Guid userId);
    Task<MethodResponse<bool>> DeleteRefreshTokenAsync(string refreshToken);
}