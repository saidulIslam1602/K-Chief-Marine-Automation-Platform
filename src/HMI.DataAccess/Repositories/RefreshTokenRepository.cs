using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HMI.Platform.Core.Models;
using HMI.DataAccess.Data;
using HMI.DataAccess.Interfaces;

namespace HMI.DataAccess.Repositories;

/// <summary>
/// Repository implementation for refresh token management.
/// </summary>
public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    private readonly ILogger<RefreshTokenRepository> _logger;

    public RefreshTokenRepository(ApplicationDbContext context, ILogger<RefreshTokenRepository> logger)
        : base(context)
    {
        _logger = logger;
    }

    /// <summary>
    /// Finds a refresh token by its hash.
    /// </summary>
    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new ArgumentException("Token hash cannot be null or empty", nameof(tokenHash));
        
        _logger.LogDebug("Repository operation: GetByTokenHash for token hash {TokenHash}", tokenHash.Length > 8 ? tokenHash[..8] + "..." : tokenHash);
        
        return await _context.Set<RefreshToken>()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    }

    /// <summary>
    /// Gets all refresh tokens for a specific user.
    /// </summary>
    public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        
        _logger.LogDebug("Repository operation: GetByUserId for user {UserId}", userId);
        
        return await _context.Set<RefreshToken>()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all active refresh tokens for a specific user.
    /// </summary>
    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        
        _logger.LogDebug("Repository operation: GetActiveTokensByUserId for user {UserId}", userId);
        
        var now = DateTime.UtcNow;
        return await _context.Set<RefreshToken>()
            .Where(t => t.UserId == userId && 
                       t.RevokedAt == null && 
                       t.UsedAt == null && 
                       t.ExpiresAt > now)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Finds a refresh token by JWT ID.
    /// </summary>
    public async Task<RefreshToken?> GetByJwtIdAsync(string jwtId)
    {
        if (string.IsNullOrWhiteSpace(jwtId))
            throw new ArgumentException("JWT ID cannot be null or empty", nameof(jwtId));
        
        _logger.LogDebug("Repository operation: GetByJwtId for JWT {JwtId}", jwtId);
        
        return await _context.Set<RefreshToken>()
            .FirstOrDefaultAsync(t => t.JwtId == jwtId);
    }

    /// <summary>
    /// Marks a refresh token as used.
    /// </summary>
    public async Task<bool> MarkAsUsedAsync(string tokenId, string? usedByIp = null)
    {
        if (string.IsNullOrWhiteSpace(tokenId))
            throw new ArgumentException("Token ID cannot be null or empty", nameof(tokenId));
        
        _logger.LogDebug("Repository operation: MarkAsUsed for token {TokenId}", tokenId);
        
        var token = await GetByIdAsync(tokenId);
        if (token == null)
        {
            _logger.LogWarning("Refresh token not found for marking as used: {TokenId}", tokenId);
            return false;
        }

        if (!token.IsActive)
        {
            _logger.LogWarning("Attempted to mark inactive refresh token as used: {TokenId}", tokenId);
            return false;
        }

        token.UsedAt = DateTime.UtcNow;
        token.UsedByIp = usedByIp;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogDebug("Marked refresh token as used: {TokenId}", tokenId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark refresh token as used: {TokenId}", tokenId);
            return false;
        }
    }

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    public async Task<bool> RevokeAsync(string tokenId, string reason, string? revokedByIp = null, string? replacedByToken = null)
    {
        if (string.IsNullOrWhiteSpace(tokenId))
            throw new ArgumentException("Token ID cannot be null or empty", nameof(tokenId));
        
        _logger.LogDebug("Repository operation: Revoke for token {TokenId}", tokenId);
        
        var token = await GetByIdAsync(tokenId);
        if (token == null)
        {
            _logger.LogWarning("Refresh token not found for revocation: {TokenId}", tokenId);
            return false;
        }

        if (token.IsRevoked)
        {
            _logger.LogWarning("Attempted to revoke already revoked refresh token: {TokenId}", tokenId);
            return false;
        }

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedReason = reason;
        token.RevokedByIp = revokedByIp;
        token.ReplacedByToken = replacedByToken;

        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Revoked refresh token: {TokenId}, Reason: {Reason}", tokenId, reason);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke refresh token: {TokenId}", tokenId);
            return false;
        }
    }

    /// <summary>
    /// Revokes all refresh tokens for a specific user.
    /// </summary>
    public async Task<int> RevokeAllUserTokensAsync(string userId, string reason, string? revokedByIp = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        
        _logger.LogDebug("Repository operation: RevokeAllUserTokens for user {UserId}", userId);
        
        var activeTokens = await GetActiveTokensByUserIdAsync(userId);
        var revokedCount = 0;

        foreach (var token in activeTokens)
        {
            if (await RevokeAsync(token.Id, reason, revokedByIp))
            {
                revokedCount++;
            }
        }

        _logger.LogInformation("Revoked {Count} refresh tokens for user {UserId}", revokedCount, userId);
        return revokedCount;
    }

    /// <summary>
    /// Gets expired refresh tokens that need cleanup.
    /// </summary>
    public async Task<IEnumerable<RefreshToken>> GetExpiredTokensAsync(int olderThanDays = 30)
    {
        _logger.LogDebug("Repository operation: GetExpiredTokens for days:{Days}", olderThanDays);
        
        var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
        
        return await _context.Set<RefreshToken>()
            .Where(t => t.ExpiresAt < cutoffDate)
            .OrderBy(t => t.ExpiresAt)
            .ToListAsync();
    }

    /// <summary>
    /// Removes expired refresh tokens from the database.
    /// </summary>
    public async Task<int> CleanupExpiredTokensAsync(int olderThanDays = 30)
    {
        _logger.LogDebug("Repository operation: CleanupExpiredTokens for days:{Days}", olderThanDays);
        
        var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
        
        try
        {
            var expiredTokens = await _context.Set<RefreshToken>()
                .Where(t => t.ExpiresAt < cutoffDate)
                .ToListAsync();

            if (expiredTokens.Any())
            {
                _context.Set<RefreshToken>().RemoveRange(expiredTokens);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Cleaned up {Count} expired refresh tokens", expiredTokens.Count);
                return expiredTokens.Count;
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired refresh tokens");
            return 0;
        }
    }
}
