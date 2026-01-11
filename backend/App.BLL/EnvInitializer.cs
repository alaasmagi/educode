using Microsoft.Extensions.Logging;

namespace App.BLL;

public class EnvInitializer
{
    private readonly ILogger<EnvInitializer> _logger;
    
    // DB
    public string PgDbConnection { get; private set; } = string.Empty;
    public string RedisConnection { get; private set; } = string.Empty;
    
    // JWT
    public string JwtKey { get; private set; } = string.Empty;
    public string JwtAudience { get; private set; } = string.Empty;
    public string JwtIssuer { get; private set; } = string.Empty;
    public int JwtExpirationMinutes { get; private set; }
    public int JwtCookieExpirationMinutes { get; private set; }

    // RefreshToken
    public int RefreshTokenExpirationDays { get; private set; }
    public int RefreshTokenCookieExpirationDays { get; private set; }
    
    // Admin
    public string AdminUserBcrypt { get; private set; } = string.Empty;
    public string AdminKeyBcrypt { get; private set; } = string.Empty;

    // OTP
    public string OtpKey { get; private set; } = string.Empty;
    public int OtpExpirationMinutes { get; private set; }

    // Mail
    public string MailSenderEmail { get; private set; } = string.Empty;
    public string MailSenderKey { get; private set; } = string.Empty;
    public string MailSenderHost { get; private set; } = string.Empty;
    public string MailSenderPort { get; private set; } = string.Empty;
    
    // OCI (Oracle Cloud Infrastructure)
    public string OciKey { get; private set; } = string.Empty;
    public string OciTenancyId { get; private set; } = string.Empty;
    public string OciUserId { get; private set; } = string.Empty;
    public string OciFingerprint { get; private set; } = string.Empty;
    public string OciRegion { get; private set; } = string.Empty;
    public string OciBucketName { get; private set; } = string.Empty;
    public string OciPublicUrl { get; private set; } = string.Empty;
    
    // Soft deletion
    public int SoftDeleteExpirationDays { get; private set; }

    // Frontend
    public string FrontendUrl { get; private set; } = string.Empty;

    
    public EnvInitializer(ILogger<EnvInitializer> logger)
    {
        _logger = logger;
    }

    public void InitializeEnv()
    {
        PgDbConnection = GetStringEnv("PG_DB_CONNECTION");
        RedisConnection = GetStringEnv("REDIS_CONNECTION");
        
        OtpKey = GetStringEnv("OTPKEY");
        OtpExpirationMinutes = GetIntEnv("OTP_MINUTES");

        AdminUserBcrypt = GetStringEnv("ADMIN_USER_BCRYPT");
        AdminKeyBcrypt = GetStringEnv("ADMIN_KEY_BCRYPT");

        RefreshTokenExpirationDays = GetIntEnv("REFRESH_TOKEN_DAYS");
        RefreshTokenCookieExpirationDays = GetIntEnv("REFRESH_TOKEN_COOKIE_DAYS");

        JwtKey = GetStringEnv("JWTKEY");
        JwtAudience = GetStringEnv("JWTAUD");
        JwtIssuer = GetStringEnv("JWTISS");
        JwtExpirationMinutes = GetIntEnv("JWT_MINUTES");
        JwtCookieExpirationMinutes = GetIntEnv("JWT_COOKIE_MINUTES");

        MailSenderEmail = GetStringEnv("MAILSENDER_EMAIL");
        MailSenderKey = GetStringEnv("MAILSENDER_KEY");
        MailSenderHost = GetStringEnv("MAILSENDER_HOST");
        MailSenderPort = GetStringEnv("MAILSENDER_PORT");
        
        OciKey = GetStringEnv("OCI_KEY");
        OciTenancyId = GetStringEnv("OCI_TENANCY_ID");
        OciUserId = GetStringEnv("OCI_USER_ID");
        OciFingerprint = GetStringEnv("OCI_FINGERPRINT");
        OciRegion = GetStringEnv("OCI_REGION");
        OciBucketName = GetStringEnv("OCI_BUCKET_NAME");
        OciPublicUrl = GetStringEnv("OCI_PUBLIC_URL");

        SoftDeleteExpirationDays = GetIntEnv("SOFTDELETE_EXPIRATION_DAYS");
        
        FrontendUrl = GetStringEnv("FRONTENDURL");

        _logger.LogInformation("Environment variables initialized.");
    }
    
    private string GetStringEnv(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            _logger.LogWarning($"Environment variable '{key}' is missing or empty.");
            return string.Empty;
        }
        return value;
    }

    private int GetIntEnv(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (int.TryParse(value, out var result))
        {
            return result;
        }
        _logger.LogWarning($"Environment variable '{key}' is missing or not an integer. Using 0 as default.");
        return 0;
    }
}