using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HMI.Platform.Core.Models;
using HMI.DataAccess.Data;
using HMI.DataAccess.Interfaces;

namespace HMI.DataAccess.Repositories;

/// <summary>
/// Repository implementation for API key management.
/// </summary>
public class ApiKeyRepository : Repository<ApiKey>, IApiKeyRepository
{
    private readonly ILogger<ApiKeyRepository> _logger;

    public ApiKeyRepository(ApplicationDbContext context, ILogger<ApiKeyRepository> logger)
        : base(context)
    {
        _logger = logger;
    }

    /// <summary>
    /// Finds an API key by its hash.
    /// </summary>
    public async Task<ApiKey?> GetByKeyHashAsync(string keyHash)
    {
        if (string.IsNullOrWhiteSpace(keyHash))
            throw new ArgumentException("Key hash cannot be null or empty", nameof(keyHash));
        
        _logger.LogDebug("Repository operation: GetByKeyHash for key hash {KeyHash}", keyHash.Length > 8 ? keyHash[..8] + "..." : keyHash);
        
        return await _context.Set<ApiKey>()
            .FirstOrDefaultAsync(k => k.KeyHash == keyHash && k.IsActive);
    }

    /// <summary>
    /// Gets all API keys for a specific owner.
    /// </summary>
    public async Task<IEnumerable<ApiKey>> GetByOwnerIdAsync(string ownerId)
    {
        if (string.IsNullOrWhiteSpace(ownerId))
            throw new ArgumentException("Owner ID cannot be null or empty", nameof(ownerId));
        
        _logger.LogDebug("Repository operation: GetByOwnerId for owner {OwnerId}", ownerId);
        
        return await _context.Set<ApiKey>()
            .Where(k => k.OwnerId == ownerId)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all active API keys.
    /// </summary>
    public async Task<IEnumerable<ApiKey>> GetActiveKeysAsync()
    {
        _logger.LogDebug("Repository operation: GetActiveKeys for all keys");
        
        return await _context.Set<ApiKey>()
            .Where(k => k.IsActive && (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow))
            .OrderByDescending(k => k.LastUsedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Updates the last used timestamp and usage count for an API key.
    /// </summary>
    public async Task<bool> UpdateUsageAsync(string apiKeyId)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            throw new ArgumentException("API Key ID cannot be null or empty", nameof(apiKeyId));
        
        _logger.LogDebug("Repository operation: UpdateUsage for API key {ApiKeyId}", apiKeyId);
        
        var apiKey = await GetByIdAsync(apiKeyId);
        if (apiKey == null)
        {
            _logger.LogWarning("API key not found for usage update: {ApiKeyId}", apiKeyId);
            return false;
        }

        apiKey.LastUsedAt = DateTime.UtcNow;
        apiKey.UsageCount++;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogDebug("Updated usage for API key {ApiKeyId}. Usage count: {UsageCount}", 
                apiKeyId, apiKey.UsageCount);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update usage for API key {ApiKeyId}", apiKeyId);
            return false;
        }
    }

    /// <summary>
    /// Deactivates an API key.
    /// </summary>
    public async Task<bool> DeactivateAsync(string apiKeyId)
    {
        if (string.IsNullOrWhiteSpace(apiKeyId))
            throw new ArgumentException("API Key ID cannot be null or empty", nameof(apiKeyId));
        
        _logger.LogDebug("Repository operation: Deactivate for API key {ApiKeyId}", apiKeyId);
        
        var apiKey = await GetByIdAsync(apiKeyId);
        if (apiKey == null)
        {
            _logger.LogWarning("API key not found for deactivation: {ApiKeyId}", apiKeyId);
            return false;
        }

        apiKey.IsActive = false;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deactivated API key {ApiKeyId}", apiKeyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate API key {ApiKeyId}", apiKeyId);
            return false;
        }
    }

    /// <summary>
    /// Gets API keys that are about to expire.
    /// </summary>
    public async Task<IEnumerable<ApiKey>> GetExpiringKeysAsync(int daysBeforeExpiration = 30)
    {
        _logger.LogDebug("Repository operation: GetExpiringKeys for days:{Days}", daysBeforeExpiration);
        
        var expirationThreshold = DateTime.UtcNow.AddDays(daysBeforeExpiration);
        
        return await _context.Set<ApiKey>()
            .Where(k => k.IsActive && 
                       k.ExpiresAt != null && 
                       k.ExpiresAt <= expirationThreshold && 
                       k.ExpiresAt > DateTime.UtcNow)
            .OrderBy(k => k.ExpiresAt)
            .ToListAsync();
    }
}
