using System.Security.Cryptography;
using App.Application.Contracts.Services;
using App.Application.Initializers;
using App.Contracts.Repositories;
using App.Infrastructure.Helpers;
using App.Infrastructure.Sentry;
using Microsoft.Extensions.Logging;

namespace App.Application.Services;

public class OtpService(
    ILogger<OtpService> logger,
    SentryService sentry,
    EnvInitializer envInitializer,
    ICacheRepository cacheRepository) : IOtpService
{
    public async Task<int> GenerateAndStoreOtp(string email)
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
        }
        
        logger.LogInformation($"Otp generation successful for user with email {email}");
        return otp;
    }

    public async Task<bool> VerifyOtp(string email, string otpToVerify)
    {
        var originalOtp =  await cacheRepository.GetAsync(Constants.OtpPrefix + email);

        if (originalOtp == null)
        {
            logger.LogWarning($"Otp verification failed for user with email {email}");
            return false;
        }

        if (originalOtp == otpToVerify)
        {
            logger.LogWarning($"Otp verification successful for user with email {email}");
            return true;
        }

        logger.LogWarning($"Otp verification failed for user with id {email}");
        return false;
    }
}