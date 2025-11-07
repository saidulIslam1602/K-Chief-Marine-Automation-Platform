using HMI.Platform.Core.Models;

namespace HMI.DataAccess.Interfaces;

/// <summary>
/// Repository interface for API key management.
/// </summary>
public interface IApiKeyRepository : IRepository<ApiKey>
{
    /// <summary>
    /// Finds an API key by its hash.
    /// </summary>
    /// <param name="keyHash">The hashed API key value.</param>
    /// <returns>The API key if found, null otherwise.</returns>
    Task<ApiKey?> GetByKeyHashAsync(string keyHash);

    /// <summary>
    /// Gets all API keys for a specific owner.
    /// </summary>
    /// <param name="ownerId">The owner ID.</param>
    /// <returns>Collection of API keys for the owner.</returns>
    Task<IEnumerable<ApiKey>> GetByOwnerIdAsync(string ownerId);

    /// <summary>
    /// Gets all active API keys.
    /// </summary>
    /// <returns>Collection of active API keys.</returns>
    Task<IEnumerable<ApiKey>> GetActiveKeysAsync();

    /// <summary>
    /// Updates the last used timestamp and usage count for an API key.
    /// </summary>
    /// <param name="apiKeyId">The API key ID.</param>
    /// <returns>True if updated successfully.</returns>
    Task<bool> UpdateUsageAsync(string apiKeyId);

    /// <summary>
    /// Deactivates an API key.
    /// </summary>
    /// <param name="apiKeyId">The API key ID.</param>
    /// <returns>True if deactivated successfully.</returns>
    Task<bool> DeactivateAsync(string apiKeyId);

    /// <summary>
    /// Gets API keys that are about to expire.
    /// </summary>
    /// <param name="daysBeforeExpiration">Number of days before expiration to check.</param>
    /// <returns>Collection of API keys expiring soon.</returns>
    Task<IEnumerable<ApiKey>> GetExpiringKeysAsync(int daysBeforeExpiration = 30);
}
