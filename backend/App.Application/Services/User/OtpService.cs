using System.Security.Cryptography;
using App.Contracts.Repositories;
using App.Contracts.Services;
using App.Infrastructure.Helpers;
using Base.Domain;
using Base.DTO;
using Microsoft.Extensions.Logging;

namespace App.Application.Services.User;

public class OtpService(
    ILogger<OtpService> logger,
    ICacheRepository cacheRepository) : IOtpService
{
    public async Task<MethodResponse<int>> GenerateAndStoreOtp(string email)
    {
        var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var otp = BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF;

        var otpExpirationMinutes = 5;
        var status = await cacheRepository.SetAsync(Constants.OtpPrefix + email, otp.ToString(), 
                                                                    TimeSpan.FromMinutes(otpExpirationMinutes));
        if (status == null)
        {
            logger.LogWarning($"Otp generation failed for user with email {email}");
            return MethodResponse<int>.Failure(
                new Error(ErrorConstants.OtpGenerationFailed, "OTP generation failed")
            );
        }
        
        logger.LogInformation($"Otp generation successful for user with email {email}");
        return MethodResponse<int>.Success(otp);
    }

    public async Task<MethodResponse<bool>> VerifyOtp(string email, string otpToVerify)
    {
        var originalOtp =  await cacheRepository.GetAsync(Constants.OtpPrefix + email);

        if (originalOtp == null)
        {
            logger.LogWarning($"OTP not found or expired for user with email {email}");
            return MethodResponse<bool>.Failure(
                new Error(ErrorConstants.OtpNotFound, "OTP not found or expired")
            );
        }

        if (originalOtp == otpToVerify)
        {
            logger.LogInformation($"Successfully verified OTP for user with email {email}");
            return MethodResponse<bool>.Success(true);
        }

        logger.LogWarning($"Invalid OTP provided for user with email {email}");
        return MethodResponse<bool>.Failure(
            new Error(ErrorConstants.OtpVerificationFailed, "OTP verification failed")
        );
    }
}