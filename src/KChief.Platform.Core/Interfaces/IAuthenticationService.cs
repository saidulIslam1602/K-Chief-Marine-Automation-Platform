using KChief.Platform.Core.Models;

namespace KChief.Platform.Core.Interfaces;

/// <summary>
/// Interface for authentication services.
/// </summary>
public interface IKChiefAuthenticationService
{
    /// <summary>
    /// Authenticates a user with username and password.
    /// </summary>
    /// <param name="username">Username or email</param>
    /// <param name="password">Plain text password</param>
    /// <returns>Authentication result with user information and token</returns>
    Task<AuthenticationResult> AuthenticateAsync(string username, string password);

    /// <summary>
    /// Authenticates using an API key.
    /// </summary>
    /// <param name="apiKey">API key value</param>
    /// <returns>Authentication result with API key information</returns>
    Task<AuthenticationResult> AuthenticateApiKeyAsync(string apiKey);

    /// <summary>
    /// Validates a JWT token.
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>Token validation result</returns>
    Task<KChiefTokenValidationResult> ValidateTokenAsync(string token);

    /// <summary>
    /// Generates a JWT token for a user.
    /// </summary>
    /// <param name="user">User to generate token for</param>
    /// <returns>JWT token</returns>
    Task<string> GenerateTokenAsync(User user);

    /// <summary>
    /// Refreshes an existing JWT token.
    /// </summary>
    /// <param name="refreshToken">Refresh token</param>
    /// <returns>New JWT token</returns>
    Task<string> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Revokes a JWT token or refresh token.
    /// </summary>
    /// <param name="token">Token to revoke</param>
    /// <returns>True if token was revoked successfully</returns>
    Task<bool> RevokeTokenAsync(string token);

    /// <summary>
    /// Hashes a password using BCrypt.
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash.
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="hash">Password hash</param>
    /// <returns>True if password matches hash</returns>
    bool VerifyPassword(string password, string hash);
}

/// <summary>
/// Result of an authentication attempt.
/// </summary>
public class AuthenticationResult
{
    /// <summary>
    /// Indicates if authentication was successful.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Error message if authentication failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Authenticated user (null if authentication failed).
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// API key information (for API key authentication).
    /// </summary>
    public ApiKey? ApiKey { get; set; }

    /// <summary>
    /// JWT access token.
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Refresh token for token renewal.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Token expiration time.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Authentication method used.
    /// </summary>
    public string? AuthenticationMethod { get; set; }
}

/// <summary>
/// Result of token validation.
/// </summary>
public class KChiefTokenValidationResult
{
    /// <summary>
    /// Indicates if token is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Error message if validation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// User ID from the token.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Username from the token.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// User role from the token.
    /// </summary>
    public UserRole? Role { get; set; }

    /// <summary>
    /// Token expiration time.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Claims from the token.
    /// </summary>
    public Dictionary<string, string> Claims { get; set; } = new();
}
