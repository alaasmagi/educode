using System.Security.Cryptography;
using App.DAL.EF;
using App.Domain;
using Contracts;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace App.BLL;

public class OtpService : IOtpService
{
    private readonly ILogger<OtpService> _logger;
    private readonly RedisRepository _redisRepository;
    
    public OtpService(IConnectionMultiplexer redis, ILogger<OtpService> logger, IConnectionMultiplexer connectionMultiplexer, ILogger<RedisRepository> redisLogger)
    {
        _logger = logger;
        _redisRepository = new RedisRepository(connectionMultiplexer, redisLogger);
    }
    
    public async Task<int> GenerateAndStoreOtp(string email)
    {
        var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var otp = BitConverter.ToInt32(bytes, 0) & 0x7FFFFFFF;

        var otpExpirationMinutes = 5; // TODO: ENV!
        var status = await _redisRepository.SetDataAsync(Constants.OtpPrefix + email, otp.ToString(), 
                                                                    TimeSpan.FromMinutes(otpExpirationMinutes));
        if (status == false)
        {
            _logger.LogWarning($"Otp generation failed for user with email {email}");
        }
        
        _logger.LogInformation($"Otp generation successful for user with email {email}");
        return otp;
    }

    public async Task<bool> VerifyOtp(string email, string otpToVerify)
    {
        var originalOtp =  await _redisRepository.GetDataAsync(Constants.OtpPrefix + email);

        if (originalOtp == null)
        {
            _logger.LogWarning($"Otp verification failed for user with email {email}");
            return false;
        }

        if (originalOtp == otpToVerify)
        {
            _logger.LogWarning($"Otp verification successful for user with email {email}");
            return true;
        }

        _logger.LogWarning($"Otp verification failed for user with id {email}");
        return false;
    }
}