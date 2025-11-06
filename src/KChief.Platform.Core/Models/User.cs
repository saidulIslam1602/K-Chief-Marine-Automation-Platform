using System.ComponentModel.DataAnnotations;

namespace KChief.Platform.Core.Models;

/// <summary>
/// Represents a user in the K-Chief Marine Automation Platform.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Username for authentication.
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// User's email address.
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password for authentication.
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// User's first name.
    /// </summary>
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name.
    /// </summary>
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User's role in the system.
    /// </summary>
    [Required]
    public UserRole Role { get; set; } = UserRole.Operator;

    /// <summary>
    /// Department or organization the user belongs to.
    /// </summary>
    [StringLength(100)]
    public string? Department { get; set; }

    /// <summary>
    /// User's job title or position.
    /// </summary>
    [StringLength(100)]
    public string? JobTitle { get; set; }

    /// <summary>
    /// Phone number for contact.
    /// </summary>
    [Phone]
    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Indicates if the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates if the user's email is verified.
    /// </summary>
    public bool EmailVerified { get; set; } = false;

    /// <summary>
    /// Date and time when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the user was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time of the user's last login.
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Number of failed login attempts.
    /// </summary>
    public int FailedLoginAttempts { get; set; } = 0;

    /// <summary>
    /// Date and time when the account was locked (if applicable).
    /// </summary>
    public DateTime? LockedUntil { get; set; }

    /// <summary>
    /// Additional metadata about the user.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();

    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Checks if the user account is currently locked.
    /// </summary>
    public bool IsLocked => LockedUntil.HasValue && LockedUntil > DateTime.UtcNow;
}
