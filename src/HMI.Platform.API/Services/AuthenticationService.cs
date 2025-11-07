using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using Serilog;
using Serilog.Context;
using HMI.Platform.Core.Interfaces;
using HMI.Platform.Core.Models;
using HMI.Platform.Core.Exceptions;
using HMI.DataAccess.Interfaces;

namespace HMI.Platform.API.Services;

/// <summary>
/// Service for handling authentication operations including JWT tokens and API keys.
/// </summary>
public class AuthenticationService : IHMIAuthenticationService
{
    private readonly IUserService _userService;
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly string _jwtSecret;
    private readonly string _jwtIssuer;
    private readonly string _jwtAudience;
    private readonly int _jwtExpirationMinutes;

    public AuthenticationService(
        IUserService userService,
        IApiKeyRepository apiKeyRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration,
        ILogger<AuthenticationService> logger)
    {
        _userService = userService;
        _apiKeyRepository = apiKeyRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
        _logger = logger;
        
        _jwtSecret = _configuration["Authentication:JWT:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
        _jwtIssuer = _configuration["Authentication:JWT:Issuer"] ?? "HMI.Platform.API";
        _jwtAudience = _configuration["Authentication:JWT:Audience"] ?? "HMI.Platform.API";
        _jwtExpirationMinutes = _configuration.GetValue<int>("Authentication:JWT:ExpirationMinutes", 60);
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string username, string password)
    {
        using (LogContext.PushProperty("Username", username))
        using (LogContext.PushProperty("AuthenticationMethod", "Password"))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    Log.Warning("Authentication failed: Username or password is empty");
                    return new AuthenticationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Username and password are required"
                    };
                }

                // Try to find user by username or email
                var user = await _userService.GetUserByUsernameAsync(username) ??
                          await _userService.GetUserByEmailAsync(username);

                if (user == null)
                {
                    Log.Warning("Authentication failed: User not found");
                    await Task.Delay(1000); // Prevent timing attacks
                    return new AuthenticationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid username or password"
                    };
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    Log.Warning("Authentication failed: User account is inactive for {UserId}", user.Id);
                    return new AuthenticationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "User account is inactive"
                    };
                }

                // Check if user is locked
                if (user.IsLocked)
                {
                    Log.Warning("Authentication failed: User account is locked for {UserId}", user.Id);
                    return new AuthenticationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "User account is locked"
                    };
                }

                // Verify password
                if (!VerifyPassword(password, user.PasswordHash))
                {
                    Log.Warning("Authentication failed: Invalid password for {UserId}", user.Id);
                    await _userService.RecordFailedLoginAsync(username);
                    return new AuthenticationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid username or password"
                    };
                }

                // Authentication successful
                await _userService.RecordSuccessfulLoginAsync(user.Id);
                var token = await GenerateTokenAsync(user);
                var refreshToken = GenerateRefreshToken();

                Log.Information("User {UserId} authenticated successfully", user.Id);

                return new AuthenticationResult
                {
                    IsSuccess = true,
                    User = user,
                    AccessToken = token,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
                    AuthenticationMethod = "Password"
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during password authentication for {Username}", username);
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Authentication failed due to an internal error"
                };
            }
        }
    }

    public async Task<AuthenticationResult> AuthenticateApiKeyAsync(string apiKey)
    {
        using (LogContext.PushProperty("AuthenticationMethod", "ApiKey"))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    Log.Warning("API key authentication failed: API key is empty");
                    return new AuthenticationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "API key is required"
                    };
                }

                // Hash the provided API key to compare with stored hash
                var apiKeyHash = HashPassword(apiKey);
                
                // Find API key by hash in repository
                var storedApiKey = await _apiKeyRepository.GetByKeyHashAsync(apiKeyHash);
                
                if (storedApiKey == null || !storedApiKey.IsValid)
                {
                    Log.Warning("Invalid or expired API key attempted");
                    return new AuthenticationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = "Invalid or expired API key"
                    };
                }

                // Update usage statistics
                await _apiKeyRepository.UpdateUsageAsync(storedApiKey.Id);
                
                Log.Information("API key authentication successful for key {ApiKeyName}", storedApiKey.Name);
                
                return new AuthenticationResult
                {
                    IsSuccess = true,
                    ApiKey = storedApiKey
                };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during API key authentication");
                return new AuthenticationResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Authentication failed due to an internal error"
                };
            }
        }
    }

    public Task<HMITokenValidationResult> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSecret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtIssuer,
                ValidateAudience = true,
                ValidAudience = _jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            var jwtToken = (JwtSecurityToken)validatedToken;

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = principal.FindFirst(ClaimTypes.Name)?.Value;
            var roleString = principal.FindFirst(ClaimTypes.Role)?.Value;
            
            Enum.TryParse<UserRole>(roleString, out var role);

            var claims = principal.Claims.ToDictionary(c => c.Type, c => c.Value);

            return Task.FromResult(new HMITokenValidationResult
            {
                IsValid = true,
                UserId = userId,
                Username = username,
                Role = role,
                ExpiresAt = jwtToken.ValidTo,
                Claims = claims
            });
        }
        catch (SecurityTokenExpiredException)
        {
            return Task.FromResult(new HMITokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Token has expired"
            });
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            return Task.FromResult(new HMITokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Token signature is invalid"
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error validating JWT token");
            return Task.FromResult(new HMITokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Token validation failed"
            });
        }
    }

    public Task<string> GenerateTokenAsync(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);

        var jwtId = Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new("FullName", user.FullName),
            new("Department", user.Department ?? ""),
            new("JobTitle", user.JobTitle ?? "")
        };

        // Add custom claims based on role
        switch (user.Role)
        {
            case UserRole.Administrator:
                claims.Add(new Claim("Permission", "FullAccess"));
                break;
            case UserRole.FleetManager:
                claims.Add(new Claim("Permission", "FleetManagement"));
                claims.Add(new Claim("Permission", "VesselControl"));
                break;
            case UserRole.Captain:
                claims.Add(new Claim("Permission", "VesselControl"));
                claims.Add(new Claim("Permission", "Navigation"));
                break;
            case UserRole.ChiefEngineer:
                claims.Add(new Claim("Permission", "EngineControl"));
                claims.Add(new Claim("Permission", "Maintenance"));
                break;
            case UserRole.NavigationOfficer:
                claims.Add(new Claim("Permission", "Navigation"));
                claims.Add(new Claim("Permission", "RouteManagement"));
                break;
            case UserRole.EngineOperator:
                claims.Add(new Claim("Permission", "EngineControl"));
                break;
            case UserRole.Operator:
                claims.Add(new Claim("Permission", "BasicControl"));
                break;
            case UserRole.Observer:
                claims.Add(new Claim("Permission", "ReadOnly"));
                break;
            case UserRole.Maintenance:
                claims.Add(new Claim("Permission", "Maintenance"));
                claims.Add(new Claim("Permission", "Diagnostics"));
                break;
            case UserRole.ShoreSupport:
                claims.Add(new Claim("Permission", "RemoteMonitoring"));
                break;
            case UserRole.Guest:
                claims.Add(new Claim("Permission", "LimitedReadOnly"));
                break;
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtExpirationMinutes),
            Issuer = _jwtIssuer,
            Audience = _jwtAudience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Task.FromResult(tokenHandler.WriteToken(token));
    }

    public async Task<string> RefreshTokenAsync(string refreshToken)
    {
        using (LogContext.PushProperty("Operation", "RefreshToken"))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(refreshToken))
                {
                    throw new ArgumentException("Refresh token is required", nameof(refreshToken));
                }

                // Hash the refresh token to find it in the database
                var tokenHash = HashPassword(refreshToken);
                var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

                if (storedToken == null || !storedToken.IsActive)
                {
                    Log.Warning("Invalid or expired refresh token attempted");
                    throw new SecurityTokenException("Invalid or expired refresh token");
                }

                // Get the user
                var user = await _userService.GetUserByIdAsync(storedToken.UserId);
                if (user == null || !user.IsActive)
                {
                    Log.Warning("Refresh token belongs to inactive user: {UserId}", storedToken.UserId);
                    throw new SecurityTokenException("User account is inactive");
                }

                // Mark the old token as used
                await _refreshTokenRepository.MarkAsUsedAsync(storedToken.Id);

                // Generate new JWT token
                var newJwtToken = await GenerateTokenAsync(user);

                // Create new refresh token
                var newRefreshToken = GenerateRefreshToken();
                var newRefreshTokenHash = HashPassword(newRefreshToken);

                var refreshTokenEntity = new RefreshToken
                {
                    TokenHash = newRefreshTokenHash,
                    UserId = user.Id,
                    JwtId = ExtractJwtId(newJwtToken),
                    ExpiresAt = DateTime.UtcNow.AddDays(_configuration.GetValue<int>("Authentication:JWT:RefreshTokenExpirationDays", 7)),
                    CreatedByIp = GetCurrentIpAddress()
                };

                await _refreshTokenRepository.AddAsync(refreshTokenEntity);

                // Revoke the old token and replace with new one
                await _refreshTokenRepository.RevokeAsync(storedToken.Id, "Replaced by new token", 
                    GetCurrentIpAddress(), newRefreshTokenHash);

                Log.Information("Refresh token successfully renewed for user {UserId}", user.Id);
                return newJwtToken;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during token refresh");
                throw;
            }
        }
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        using (LogContext.PushProperty("Operation", "RevokeToken"))
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new ArgumentException("Token is required", nameof(token));
                }

                // Try to parse the JWT to get the JTI claim
                var jwtId = ExtractJwtId(token);
                if (string.IsNullOrEmpty(jwtId))
                {
                    Log.Warning("Invalid JWT token provided for revocation");
                    return false;
                }

                // Find and revoke the refresh token associated with this JWT
                var refreshToken = await _refreshTokenRepository.GetByJwtIdAsync(jwtId);
                if (refreshToken != null && refreshToken.IsActive)
                {
                    await _refreshTokenRepository.RevokeAsync(refreshToken.Id, "Token revoked by user", GetCurrentIpAddress());
                    Log.Information("Refresh token revoked for JWT ID: {JwtId}", jwtId);
                }

                // In a production system, you would also add the JWT to a blacklist
                // For now, we'll just revoke the associated refresh token
                Log.Information("Token revocation completed for JWT ID: {JwtId}", jwtId);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during token revocation");
                return false;
            }
        }
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error verifying password hash");
            return false;
        }
    }

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
    }

    private string ExtractJwtId(string jwtToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(jwtToken);
            return jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value ?? string.Empty;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error extracting JWT ID from token");
            return string.Empty;
        }
    }

    private string? GetCurrentIpAddress()
    {
        // In a real implementation, you would get this from HttpContext
        // For now, return null as it's optional
        return null;
    }
}
