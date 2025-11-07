using HMI.Platform.Core.Models;

namespace HMI.DataAccess.Interfaces;

/// <summary>
/// Repository interface for refresh token management.
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>
    /// Finds a refresh token by its hash.
    /// </summary>
    /// <param name="tokenHash">The hashed token value.</param>
    /// <returns>The refresh token if found, null otherwise.</returns>
    Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);

    /// <summary>
    /// Gets all refresh tokens for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>Collection of refresh tokens for the user.</returns>
    Task<IEnumerable<RefreshToken>> GetByUserIdAsync(string userId);

    /// <summary>
    /// Gets all active refresh tokens for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>Collection of active refresh tokens for the user.</returns>
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);

    /// <summary>
    /// Finds a refresh token by JWT ID.
    /// </summary>
    /// <param name="jwtId">The JWT ID (jti claim).</param>
    /// <returns>The refresh token if found, null otherwise.</returns>
    Task<RefreshToken?> GetByJwtIdAsync(string jwtId);

    /// <summary>
    /// Marks a refresh token as used.
    /// </summary>
    /// <param name="tokenId">The token ID.</param>
    /// <param name="usedByIp">The IP address that used the token.</param>
    /// <returns>True if marked as used successfully.</returns>
    Task<bool> MarkAsUsedAsync(string tokenId, string? usedByIp = null);

    /// <summary>
    /// Revokes a refresh token.
    /// </summary>
    /// <param name="tokenId">The token ID.</param>
    /// <param name="reason">The reason for revocation.</param>
    /// <param name="revokedByIp">The IP address that revoked the token.</param>
    /// <param name="replacedByToken">The token that replaced this one (if any).</param>
    /// <returns>True if revoked successfully.</returns>
    Task<bool> RevokeAsync(string tokenId, string reason, string? revokedByIp = null, string? replacedByToken = null);

    /// <summary>
    /// Revokes all refresh tokens for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="reason">The reason for revocation.</param>
    /// <param name="revokedByIp">The IP address that revoked the tokens.</param>
    /// <returns>Number of tokens revoked.</returns>
    Task<int> RevokeAllUserTokensAsync(string userId, string reason, string? revokedByIp = null);

    /// <summary>
    /// Gets expired refresh tokens that need cleanup.
    /// </summary>
    /// <param name="olderThanDays">Get tokens older than this many days.</param>
    /// <returns>Collection of expired tokens.</returns>
    Task<IEnumerable<RefreshToken>> GetExpiredTokensAsync(int olderThanDays = 30);

    /// <summary>
    /// Removes expired refresh tokens from the database.
    /// </summary>
    /// <param name="olderThanDays">Remove tokens older than this many days.</param>
    /// <returns>Number of tokens removed.</returns>
    Task<int> CleanupExpiredTokensAsync(int olderThanDays = 30);
}
