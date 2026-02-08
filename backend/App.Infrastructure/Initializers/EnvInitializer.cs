using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Initializers;

// TODO: Implement proper error logging (sentry)
public class EnvInitializer(ILogger<EnvInitializer> logger)
{
    // DB
    public string PgDbConnection { get; private set; } = string.Empty;
    public string RedisConnection { get; private set; } = string.Empty;
    
    // JWT
    public string JwtKey { get; private set; } = string.Empty;
    public string JwtAudience { get; private set; } = string.Empty;
    public string JwtIssuer { get; private set; } = string.Empty;
    public int JwtExpirationMinutes { get; private set; }
    public int JwtAdminExpirationMinutes { get; private set; }
    public int JwtCookieExpirationMinutes { get; private set; }
    
    // Email API
    public string EmailApiUrl { get; private set; } = string.Empty;
    public string EmailApiKey { get; private set; } = string.Empty;
    public int EmailExpiryMinutes { get; private set; }
    
    // Sentry
    public string SentryDsn { get; private set; } = string.Empty;

    // RefreshToken
    public int RefreshTokenExpirationDays { get; private set; }
    public int RefreshTokenCookieExpirationDays { get; private set; }
    
    // Admin
    public string DefaultAdminUser { get; private set; } = string.Empty;
    public string DefaultAdminPassword { get; private set; } = string.Empty;

    // OTP
    public string OtpKey { get; private set; } = string.Empty;
    public int OtpExpirationMinutes { get; private set; }
    
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
    public string FrontendUrls { get; private set; } = string.Empty;


    public void InitializeEnv()
    {
        PgDbConnection = GetStringEnv("PG_DB_CONNECTION");
        RedisConnection = GetStringEnv("REDIS_CONNECTION");
        
        OtpKey = GetStringEnv("OTPKEY");
        OtpExpirationMinutes = GetIntEnv("OTP_MINUTES");
        
        DefaultAdminUser = GetStringEnv("DEFAULT_ADMIN_USER");
        DefaultAdminPassword = GetStringEnv("DEFAULT_ADMIN_PASSWORD");

        RefreshTokenExpirationDays = GetIntEnv("REFRESH_TOKEN_DAYS");
        RefreshTokenCookieExpirationDays = GetIntEnv("REFRESH_TOKEN_COOKIE_DAYS");

        JwtKey = GetStringEnv("JWTKEY");
        JwtAudience = GetStringEnv("JWTAUD");
        JwtIssuer = GetStringEnv("JWTISS");
        JwtExpirationMinutes = GetIntEnv("JWT_MINUTES");
        JwtAdminExpirationMinutes = GetIntEnv("JWT_ADMIN_MINUTES");
        JwtCookieExpirationMinutes = GetIntEnv("JWT_COOKIE_MINUTES");
        
        EmailApiUrl = GetStringEnv("EMAIL_API_URL");
        EmailApiKey = GetStringEnv("EMAIL_API_KEY");
        EmailExpiryMinutes = GetIntEnv("EMAIL_EXPIRY_MINUTES");
        
        SentryDsn = GetStringEnv("SENTRY_DSN");
        
        OciKey = GetStringEnv("OCI_KEY");
        OciTenancyId = GetStringEnv("OCI_TENANCY_ID");
        OciUserId = GetStringEnv("OCI_USER_ID");
        OciFingerprint = GetStringEnv("OCI_FINGERPRINT");
        OciRegion = GetStringEnv("OCI_REGION");
        OciBucketName = GetStringEnv("OCI_BUCKET_NAME");
        OciPublicUrl = GetStringEnv("OCI_PUBLIC_URL");

        SoftDeleteExpirationDays = GetIntEnv("SOFTDELETE_EXPIRATION_DAYS");
        
        FrontendUrls = GetStringEnv("FRONTENDURLS");

        logger.LogInformation("Environment variables initialized.");
    }
    
    private string GetStringEnv(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
        {
            logger.LogWarning($"Environment variable '{key}' is missing or empty.");
            return "";
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
        logger.LogWarning($"Environment variable '{key}' is missing or not an integer. Using 0 as default.");
        return 0;
    }
}