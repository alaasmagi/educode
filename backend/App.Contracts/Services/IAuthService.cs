using App.Contracts.DTOs;
using App.Contracts.WebRequests;
using App.Domain.Entities;
using Base.DTO;

namespace App.Contracts.Services;

public interface IAuthService
{
    public Task<MethodResponse<(UserDto, string, string)>> AuthenticateUserAsync(LoginRequest request, 
                                                        string clientIp, string clientApp, bool includeDeleted = false);
    public Task<MethodResponse<UserDto>> RegisterUserAsync(CreateAccountRequest request);
    public Task<MethodResponse<bool>> ChangePasswordAsync(ChangePasswordRequest request);
    public Task<MethodResponse<bool>> LogOutUserAsync(string refreshToken);
    public Task<MethodResponse<(string AccessToken, string RefreshToken)>> RefreshTokensAsync(string refreshToken, 
                                                                    string accessToken, string email, string clientApp);
    public Task<MethodResponse<bool>> GenerateAndSendOtpAsync(OtpRequest request);
    public Task<MethodResponse<(string AccessToken, string RefreshToken)>> VerifyOtpAsync(VerifyOtpRequest request, string creatorIp);
}