namespace Contracts;

public interface IOtpService
{
    Task<int> GenerateAndStoreOtp(string email);
    Task<bool> VerifyOtp(string email, string otpToVerify);
}