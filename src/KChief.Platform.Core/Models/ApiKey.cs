using System.ComponentModel.DataAnnotations;

namespace KChief.Platform.Core.Models;

/// <summary>
/// Represents an API key for service-to-service authentication.
/// </summary>
public class ApiKey
{
    /// <summary>
    /// Unique identifier for the API key.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The API key value (hashed for security).
    /// </summary>
    [Required]
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name for the API key.
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of what this API key is used for.
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// The user or service that owns this API key.
    /// </summary>
    [Required]
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// Owner type (User, Service, System).
    /// </summary>
    [Required]
    public string OwnerType { get; set; } = "User";

    /// <summary>
    /// Scopes/permissions associated with this API key.
    /// </summary>
    public List<string> Scopes { get; set; } = new();

    /// <summary>
    /// Allowed IP addresses or CIDR ranges (empty means all IPs allowed).
    /// </summary>
    public List<string> AllowedIPs { get; set; } = new();

    /// <summary>
    /// Rate limit for this API key (requests per minute).
    /// </summary>
    public int RateLimitPerMinute { get; set; } = 1000;

    /// <summary>
    /// Indicates if the API key is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date and time when the API key was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the API key expires (null means no expiration).
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Date and time when the API key was last used.
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Number of times this API key has been used.
    /// </summary>
    public long UsageCount { get; set; } = 0;

    /// <summary>
    /// Additional metadata about the API key.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Checks if the API key is currently valid (active and not expired).
    /// </summary>
    public bool IsValid => IsActive && (ExpiresAt == null || ExpiresAt > DateTime.UtcNow);
}
