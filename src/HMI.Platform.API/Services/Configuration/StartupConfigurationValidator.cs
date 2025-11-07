using Microsoft.Extensions.Options;
using HMI.Platform.Core.Configuration;
using Serilog;

namespace HMI.Platform.API.Services.Configuration;

/// <summary>
/// Validates configuration at startup to ensure all required settings are present and valid.
/// </summary>
public class StartupConfigurationValidator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public StartupConfigurationValidator(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    /// <summary>
    /// Validates all critical configuration settings.
    /// </summary>
    public void ValidateConfiguration()
    {
        var errors = new List<string>();

        // Validate JWT configuration
        ValidateJwtConfiguration(errors);

        // Validate database configuration
        ValidateDatabaseConfiguration(errors);

        // Validate authentication configuration
        ValidateAuthenticationConfiguration(errors);

        // Validate caching configuration
        ValidateCachingConfiguration(errors);

        // Validate middleware configuration
        ValidateMiddlewareConfiguration(errors);

        // Validate background services configuration
        ValidateBackgroundServicesConfiguration(errors);

        if (errors.Any())
        {
            var errorMessage = string.Join(Environment.NewLine, errors);
            Log.Fatal("Configuration validation failed:{NewLine}{Errors}", Environment.NewLine, errorMessage);
            throw new InvalidOperationException($"Configuration validation failed:{Environment.NewLine}{errorMessage}");
        }

        Log.Information("Configuration validation completed successfully");
    }

    private void ValidateJwtConfiguration(List<string> errors)
    {
        var jwtSecret = _configuration["Authentication:JWT:Secret"];
        if (string.IsNullOrWhiteSpace(jwtSecret))
        {
            errors.Add("JWT Secret is required but not configured");
        }
        else if (jwtSecret.Length < 32)
        {
            errors.Add("JWT Secret must be at least 32 characters long");
        }

        var jwtIssuer = _configuration["Authentication:JWT:Issuer"];
        if (string.IsNullOrWhiteSpace(jwtIssuer))
        {
            errors.Add("JWT Issuer is required but not configured");
        }

        var jwtAudience = _configuration["Authentication:JWT:Audience"];
        if (string.IsNullOrWhiteSpace(jwtAudience))
        {
            errors.Add("JWT Audience is required but not configured");
        }

        var expirationMinutes = _configuration.GetValue<int>("Authentication:JWT:ExpirationMinutes", 0);
        if (expirationMinutes <= 0 || expirationMinutes > 1440)
        {
            errors.Add("JWT ExpirationMinutes must be between 1 and 1440 minutes");
        }

        var refreshTokenDays = _configuration.GetValue<int>("Authentication:JWT:RefreshTokenExpirationDays", 0);
        if (refreshTokenDays <= 0 || refreshTokenDays > 365)
        {
            errors.Add("JWT RefreshTokenExpirationDays must be between 1 and 365 days");
        }
    }

    private void ValidateDatabaseConfiguration(List<string> errors)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            errors.Add("Database connection string is required but not configured");
        }
    }

    private void ValidateAuthenticationConfiguration(List<string> errors)
    {
        var maxFailedAttempts = _configuration.GetValue<int>("Authentication:Security:MaxFailedLoginAttempts", 0);
        if (maxFailedAttempts <= 0 || maxFailedAttempts > 20)
        {
            errors.Add("MaxFailedLoginAttempts must be between 1 and 20");
        }

        var lockoutMinutes = _configuration.GetValue<int>("Authentication:Security:AccountLockoutMinutes", 0);
        if (lockoutMinutes <= 0 || lockoutMinutes > 1440)
        {
            errors.Add("AccountLockoutMinutes must be between 1 and 1440 minutes");
        }

        var apiKeyRateLimit = _configuration.GetValue<int>("Authentication:ApiKey:DefaultRateLimitPerMinute", 0);
        if (apiKeyRateLimit <= 0 || apiKeyRateLimit > 10000)
        {
            errors.Add("API Key DefaultRateLimitPerMinute must be between 1 and 10000");
        }
    }

    private void ValidateCachingConfiguration(List<string> errors)
    {
        var useDistributedCache = _configuration.GetValue<bool>("Caching:UseDistributedCache", false);
        if (useDistributedCache)
        {
            var redisConnectionString = _configuration["Caching:RedisConnectionString"];
            if (string.IsNullOrWhiteSpace(redisConnectionString))
            {
                errors.Add("Redis connection string is required when UseDistributedCache is enabled");
            }
        }

        var inMemoryCacheSize = _configuration.GetValue<int>("Caching:InMemoryCacheSizeLimit", 0);
        if (inMemoryCacheSize <= 0 || inMemoryCacheSize > 100000)
        {
            errors.Add("InMemoryCacheSizeLimit must be between 1 and 100000");
        }
    }

    private void ValidateMiddlewareConfiguration(List<string> errors)
    {
        var maxRequestSize = _configuration.GetValue<long>("Middleware:RequestValidation:MaxRequestSize", 0);
        if (maxRequestSize <= 0 || maxRequestSize > 100 * 1024 * 1024) // 100MB max
        {
            errors.Add("MaxRequestSize must be between 1 and 104857600 bytes (100MB)");
        }

        var requestsPerWindow = _configuration.GetValue<int>("Middleware:RateLimiting:RequestsPerWindow", 0);
        if (requestsPerWindow <= 0 || requestsPerWindow > 10000)
        {
            errors.Add("RateLimiting RequestsPerWindow must be between 1 and 10000");
        }

        var windowSizeSeconds = _configuration.GetValue<int>("Middleware:RateLimiting:WindowSizeSeconds", 0);
        if (windowSizeSeconds <= 0 || windowSizeSeconds > 3600)
        {
            errors.Add("RateLimiting WindowSizeSeconds must be between 1 and 3600 seconds");
        }
    }

    private void ValidateBackgroundServicesConfiguration(List<string> errors)
    {
        var pollingIntervalText = _configuration["BackgroundServices:DataPolling:PollingInterval"];
        if (!string.IsNullOrWhiteSpace(pollingIntervalText))
        {
            if (!TimeSpan.TryParse(pollingIntervalText, out var pollingInterval))
            {
                errors.Add("DataPolling PollingInterval must be a valid TimeSpan format");
            }
            else if (pollingInterval.TotalSeconds < 1 || pollingInterval.TotalHours > 24)
            {
                errors.Add("DataPolling PollingInterval must be between 1 second and 24 hours");
            }
        }

        var maxConcurrentVessels = _configuration.GetValue<int>("BackgroundServices:DataPolling:MaxConcurrentVessels", 0);
        if (maxConcurrentVessels <= 0 || maxConcurrentVessels > 100)
        {
            errors.Add("DataPolling MaxConcurrentVessels must be between 1 and 100");
        }

        var healthCheckIntervalText = _configuration["BackgroundServices:PeriodicHealthCheck:CheckInterval"];
        if (!string.IsNullOrWhiteSpace(healthCheckIntervalText))
        {
            if (!TimeSpan.TryParse(healthCheckIntervalText, out var healthCheckInterval))
            {
                errors.Add("PeriodicHealthCheck CheckInterval must be a valid TimeSpan format");
            }
            else if (healthCheckInterval.TotalMinutes < 1 || healthCheckInterval.TotalHours > 24)
            {
                errors.Add("PeriodicHealthCheck CheckInterval must be between 1 minute and 24 hours");
            }
        }
    }
}
