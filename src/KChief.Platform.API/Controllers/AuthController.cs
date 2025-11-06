using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Serilog;
using Serilog.Context;
using KChief.Platform.Core.Interfaces;
using KChief.Platform.Core.Models;
using KChief.Platform.Core.Exceptions;
using KChief.Platform.API.Services;

namespace KChief.Platform.API.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IKChiefAuthenticationService _authenticationService;
    private readonly IUserService _userService;
    private readonly ErrorLoggingService _errorLoggingService;

    public AuthController(
        IKChiefAuthenticationService authenticationService,
        IUserService userService,
        ErrorLoggingService errorLoggingService)
    {
        _authenticationService = authenticationService;
        _userService = userService;
        _errorLoggingService = errorLoggingService;
    }

    /// <summary>
    /// Authenticates a user with username/email and password.
    /// </summary>
    /// <param name="request">Login request</param>
    /// <returns>Authentication result with JWT token</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        using (LogContext.PushProperty("Username", request.Username))
        using (LogContext.PushProperty("Operation", "Login"))
        {
            try
            {
                var result = await _authenticationService.AuthenticateAsync(request.Username, request.Password);

                if (result.IsSuccess && result.User != null)
                {
                    Log.Information("User login successful for {Username}", request.Username);

                    return Ok(new LoginResponse
                    {
                        AccessToken = result.AccessToken!,
                        RefreshToken = result.RefreshToken!,
                        ExpiresAt = result.ExpiresAt!.Value,
                        User = new UserInfo
                        {
                            Id = result.User.Id,
                            Username = result.User.Username,
                            Email = result.User.Email,
                            FullName = result.User.FullName,
                            Role = result.User.Role,
                            Department = result.User.Department,
                            JobTitle = result.User.JobTitle
                        }
                    });
                }

                Log.Warning("User login failed for {Username}: {ErrorMessage}", request.Username, result.ErrorMessage);
                return Unauthorized(new ProblemDetails
                {
                    Title = "Authentication Failed",
                    Detail = result.ErrorMessage,
                    Status = StatusCodes.Status401Unauthorized
                });
            }
            catch (Exception ex)
            {
                _errorLoggingService.LogException(ex, HttpContext);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred during authentication",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }

    /// <summary>
    /// Refreshes an access token using a refresh token.
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New access token</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        using (LogContext.PushProperty("Operation", "RefreshToken"))
        {
            try
            {
                var newToken = await _authenticationService.RefreshTokenAsync(request.RefreshToken);

                Log.Information("Token refresh successful");

                return Ok(new RefreshTokenResponse
                {
                    AccessToken = newToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60) // TODO: Get from configuration
                });
            }
            catch (NotImplementedException)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Feature Not Implemented",
                    Detail = "Token refresh functionality is not yet implemented",
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _errorLoggingService.LogException(ex, HttpContext);
                return Unauthorized(new ProblemDetails
                {
                    Title = "Token Refresh Failed",
                    Detail = "Unable to refresh token",
                    Status = StatusCodes.Status401Unauthorized
                });
            }
        }
    }

    /// <summary>
    /// Revokes a token (logout).
    /// </summary>
    /// <param name="request">Revoke token request</param>
    /// <returns>Success result</returns>
    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
    {
        using (LogContext.PushProperty("Operation", "RevokeToken"))
        using (LogContext.PushProperty("UserId", User.Identity?.Name))
        {
            try
            {
                var success = await _authenticationService.RevokeTokenAsync(request.Token);

                if (success)
                {
                    Log.Information("Token revocation successful for user {UserId}", User.Identity?.Name);
                    return Ok(new { message = "Token revoked successfully" });
                }

                return BadRequest(new ProblemDetails
                {
                    Title = "Token Revocation Failed",
                    Detail = "Unable to revoke token",
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (NotImplementedException)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Feature Not Implemented",
                    Detail = "Token revocation functionality is not yet implemented",
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _errorLoggingService.LogException(ex, HttpContext);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred during token revocation",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }

    /// <summary>
    /// Gets information about the current authenticated user.
    /// </summary>
    /// <returns>Current user information</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        using (LogContext.PushProperty("Operation", "GetCurrentUser"))
        using (LogContext.PushProperty("UserId", User.Identity?.Name))
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new ProblemDetails
                    {
                        Title = "User Not Found",
                        Detail = "Unable to identify current user",
                        Status = StatusCodes.Status401Unauthorized
                    });
                }

                var user = await _userService.GetUserByIdAsync(userId);
                
                if (user == null)
                {
                    return Unauthorized(new ProblemDetails
                    {
                        Title = "User Not Found",
                        Detail = "User account no longer exists",
                        Status = StatusCodes.Status401Unauthorized
                    });
                }

                return Ok(new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role,
                    Department = user.Department,
                    JobTitle = user.JobTitle
                });
            }
            catch (Exception ex)
            {
                _errorLoggingService.LogException(ex, HttpContext);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while retrieving user information",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }

    /// <summary>
    /// Changes the current user's password.
    /// </summary>
    /// <param name="request">Change password request</param>
    /// <returns>Success result</returns>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        using (LogContext.PushProperty("Operation", "ChangePassword"))
        using (LogContext.PushProperty("UserId", User.Identity?.Name))
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var success = await _userService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

                if (success)
                {
                    Log.Information("Password change successful for user {UserId}", userId);
                    return Ok(new { message = "Password changed successfully" });
                }

                return BadRequest(new ProblemDetails
                {
                    Title = "Password Change Failed",
                    Detail = "Current password is incorrect",
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _errorLoggingService.LogException(ex, HttpContext);
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An error occurred while changing password",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}

// DTOs for authentication endpoints

public class LoginRequest
{
    [Required]
    [StringLength(255, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfo User { get; set; } = new();
}

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class RevokeTokenRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
    [Required]
    [StringLength(255, MinimumLength = 6)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [StringLength(255, MinimumLength = 6)]
    public string NewPassword { get; set; } = string.Empty;
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
}
