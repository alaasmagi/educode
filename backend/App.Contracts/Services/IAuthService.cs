using App.Contracts.DTOs;
using App.Domain.Entities;
using Base.DTO;

namespace App.Contracts.Services;

public interface IAuthService
{
    public Task<MethodResponse<(UserDto, string, string)>> AuthenticateUserAsync(string email, string password, string clientIp,
                                                                            string client, bool includeDeleted = false);
    public Task<MethodResponse<UserDto>> RegisterUserAsync(string email, string fullName, string password, Guid schoolId, string client, 
                                                                                                    string? studentCode);
    public Task<MethodResponse<bool>> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, string client);
    public Task<MethodResponse<bool>> LogOutUserAsync(string refreshToken);
    public Task<MethodResponse<(string AccessToken, string RefreshToken)>> RefreshTokensAsync(string refreshToken, string accessToken, string client);
}