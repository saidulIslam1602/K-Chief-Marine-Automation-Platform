using HMI.Platform.Core.Models;

namespace HMI.Platform.Core.Interfaces;

/// <summary>
/// Interface for user management services.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User if found, null otherwise</returns>
    Task<User?> GetUserByIdAsync(string userId);

    /// <summary>
    /// Gets a user by username.
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>User if found, null otherwise</returns>
    Task<User?> GetUserByUsernameAsync(string username);

    /// <summary>
    /// Gets a user by email.
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>User if found, null otherwise</returns>
    Task<User?> GetUserByEmailAsync(string email);

    /// <summary>
    /// Gets all users with optional filtering.
    /// </summary>
    /// <param name="role">Filter by role (optional)</param>
    /// <param name="isActive">Filter by active status (optional)</param>
    /// <param name="department">Filter by department (optional)</param>
    /// <returns>List of users</returns>
    Task<IEnumerable<User>> GetUsersAsync(UserRole? role = null, bool? isActive = null, string? department = null);

    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="user">User to create</param>
    /// <param name="password">Plain text password</param>
    /// <returns>Created user</returns>
    Task<User> CreateUserAsync(User user, string password);

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    /// <param name="user">User with updated information</param>
    /// <returns>Updated user</returns>
    Task<User> UpdateUserAsync(User user);

    /// <summary>
    /// Deletes a user.
    /// </summary>
    /// <param name="userId">User ID to delete</param>
    /// <returns>True if user was deleted successfully</returns>
    Task<bool> DeleteUserAsync(string userId);

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="currentPassword">Current password</param>
    /// <param name="newPassword">New password</param>
    /// <returns>True if password was changed successfully</returns>
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

    /// <summary>
    /// Resets a user's password (admin function).
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="newPassword">New password</param>
    /// <returns>True if password was reset successfully</returns>
    Task<bool> ResetPasswordAsync(string userId, string newPassword);

    /// <summary>
    /// Locks a user account.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="lockUntil">Lock until this date/time</param>
    /// <returns>True if user was locked successfully</returns>
    Task<bool> LockUserAsync(string userId, DateTime? lockUntil = null);

    /// <summary>
    /// Unlocks a user account.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>True if user was unlocked successfully</returns>
    Task<bool> UnlockUserAsync(string userId);

    /// <summary>
    /// Records a failed login attempt.
    /// </summary>
    /// <param name="username">Username that failed to login</param>
    /// <returns>Number of failed attempts</returns>
    Task<int> RecordFailedLoginAsync(string username);

    /// <summary>
    /// Records a successful login.
    /// </summary>
    /// <param name="userId">User ID that logged in successfully</param>
    /// <returns>Updated user</returns>
    Task<User> RecordSuccessfulLoginAsync(string userId);

    /// <summary>
    /// Validates if a username is available.
    /// </summary>
    /// <param name="username">Username to check</param>
    /// <param name="excludeUserId">User ID to exclude from check (for updates)</param>
    /// <returns>True if username is available</returns>
    Task<bool> IsUsernameAvailableAsync(string username, string? excludeUserId = null);

    /// <summary>
    /// Validates if an email is available.
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="excludeUserId">User ID to exclude from check (for updates)</param>
    /// <returns>True if email is available</returns>
    Task<bool> IsEmailAvailableAsync(string email, string? excludeUserId = null);
}
