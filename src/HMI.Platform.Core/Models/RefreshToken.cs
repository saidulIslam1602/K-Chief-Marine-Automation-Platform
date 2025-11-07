using System.ComponentModel.DataAnnotations;

namespace HMI.Platform.Core.Models;

/// <summary>
/// Represents a refresh token for JWT authentication.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Unique identifier for the refresh token.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The refresh token value (hashed for security).
    /// </summary>
    [Required]
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// The user ID this token belongs to.
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The JWT ID (jti) this refresh token is associated with.
    /// </summary>
    [Required]
    public string JwtId { get; set; } = string.Empty;

    /// <summary>
    /// Date and time when the token was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the token expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Date and time when the token was used (null if never used).
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// Date and time when the token was revoked (null if not revoked).
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Reason for revocation (if revoked).
    /// </summary>
    public string? RevokedReason { get; set; }

    /// <summary>
    /// IP address from which the token was created.
    /// </summary>
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// IP address from which the token was used.
    /// </summary>
    public string? UsedByIp { get; set; }

    /// <summary>
    /// IP address from which the token was revoked.
    /// </summary>
    public string? RevokedByIp { get; set; }

    /// <summary>
    /// The refresh token that replaced this one (if any).
    /// </summary>
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// Additional metadata about the token.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Checks if the refresh token is currently active (not used, revoked, or expired).
    /// </summary>
    public bool IsActive => RevokedAt == null && UsedAt == null && DateTime.UtcNow < ExpiresAt;

    /// <summary>
    /// Checks if the refresh token is expired.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Checks if the refresh token is revoked.
    /// </summary>
    public bool IsRevoked => RevokedAt != null;

    /// <summary>
    /// Checks if the refresh token has been used.
    /// </summary>
    public bool IsUsed => UsedAt != null;
}
