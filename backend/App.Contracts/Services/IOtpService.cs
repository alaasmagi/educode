using Base.DTO;

namespace App.Contracts.Services;

public interface IOtpService
{
    Task<MethodResponse<int>> GenerateAndStoreOtp(string email);
    Task<MethodResponse<bool>> VerifyOtp(string email, string otpToVerify);
}