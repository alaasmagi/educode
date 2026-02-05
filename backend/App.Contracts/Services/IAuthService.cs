using App.Domain.Entities;

namespace App.Contracts.Services;

public interface IAuthService
{
    public Task<(UserEntity, string, string)?> AuthenticateUserAsync(string email, string password, string clientIp,
                                                                            string client, bool includeDeleted = false);
    public Task<UserEntity?> RegisterUserAsync(string email, string fullName, string password, Guid schoolId, string client, 
                                                                                                    string? studentCode);
    public Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, string client);
    public Task<bool> LogOutUserAsync(string refreshToken);
    public Task<(string AccessToken, string RefreshToken)?> RefreshTokensAsync(string refreshToken, string accessToken, string client);
}