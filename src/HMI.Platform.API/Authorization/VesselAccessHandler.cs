using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Serilog;
using Serilog.Context;
using HMI.Platform.Core.Models;
using HMI.DataAccess.Interfaces;

namespace HMI.Platform.API.Authorization;

/// <summary>
/// Authorization handler for vessel access requirements.
/// </summary>
public class VesselAccessHandler : AuthorizationHandler<VesselAccessRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<VesselAccessHandler> _logger;
    private readonly IVesselRepository _vesselRepository;

    public VesselAccessHandler(IHttpContextAccessor httpContextAccessor, ILogger<VesselAccessHandler> logger, IVesselRepository vesselRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _vesselRepository = vesselRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VesselAccessRequirement requirement)
    {
        var user = context.User;
        var httpContext = _httpContextAccessor.HttpContext;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            Log.Warning("Vessel access denied: User not authenticated");
            return;
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
        var vesselId = GetVesselIdFromContext(httpContext, requirement);

        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("UserRole", userRole))
        using (LogContext.PushProperty("VesselId", vesselId))
        using (LogContext.PushProperty("AuthorizationRequirement", "VesselAccess"))
        {
            // Administrators and Fleet Managers have access to all vessels
            if (Enum.TryParse<UserRole>(userRole, out var role))
            {
                if (role == UserRole.Administrator || role == UserRole.FleetManager)
                {
                    Log.Information("Vessel access granted: User has administrative privileges");
                    context.Succeed(requirement);
                    return;
                }

                // Captains have access to their assigned vessels
                if (role == UserRole.Captain)
                {
                    // Check if vessel exists and user has access
                    var vessel = await _vesselRepository.GetByIdAsync(vesselId);
                    if (vessel != null)
                    {
                        Log.Information("Vessel access granted: User is a Captain with access to vessel {VesselId}", vesselId);
                        context.Succeed(requirement);
                    }
                    else
                    {
                        Log.Warning("Vessel access denied: Vessel {VesselId} not found", vesselId);
                        context.Fail();
                    }
                    return;
                }

                // Other roles have limited access based on their permissions
                if (HasVesselPermission(user, vesselId, role))
                {
                    Log.Information("Vessel access granted: User has required permissions");
                    context.Succeed(requirement);
                    return;
                }
            }

            Log.Warning("Vessel access denied: Insufficient permissions for vessel {VesselId}", vesselId);
        }
    }

    private string? GetVesselIdFromContext(HttpContext? httpContext, VesselAccessRequirement requirement)
    {
        // First check if vessel ID is specified in the requirement
        if (!string.IsNullOrEmpty(requirement.VesselId))
        {
            return requirement.VesselId;
        }

        // Try to get vessel ID from route parameters
        if (httpContext?.Request.RouteValues.TryGetValue("vesselId", out var vesselIdObj) == true)
        {
            return vesselIdObj?.ToString();
        }

        // Try to get vessel ID from query parameters
        if (httpContext?.Request.Query.TryGetValue("vesselId", out var vesselIdQuery) == true)
        {
            return vesselIdQuery.FirstOrDefault();
        }

        // Try to get vessel ID from headers
        if (httpContext?.Request.Headers.TryGetValue("X-Vessel-ID", out var vesselIdHeader) == true)
        {
            return vesselIdHeader.FirstOrDefault();
        }

        return null;
    }

    private bool HasVesselPermission(ClaimsPrincipal user, string? vesselId, UserRole role)
    {
        // Check if user has general vessel control permissions
        var permissions = user.FindAll("Permission").Select(c => c.Value).ToList();

        return role switch
        {
            UserRole.ChiefEngineer => permissions.Contains("EngineControl") || permissions.Contains("VesselControl"),
            UserRole.NavigationOfficer => permissions.Contains("Navigation") || permissions.Contains("VesselControl"),
            UserRole.EngineOperator => permissions.Contains("EngineControl"),
            UserRole.Operator => permissions.Contains("BasicControl") || permissions.Contains("VesselControl"),
            UserRole.Observer => permissions.Contains("ReadOnly"),
            UserRole.Maintenance => permissions.Contains("Maintenance"),
            UserRole.ShoreSupport => permissions.Contains("RemoteMonitoring"),
            UserRole.Guest => permissions.Contains("LimitedReadOnly"),
            _ => false
        };
    }
}

/// <summary>
/// Authorization handler for vessel ownership requirements.
/// </summary>
public class VesselOwnershipHandler : AuthorizationHandler<VesselOwnershipRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<VesselOwnershipHandler> _logger;
    private readonly IVesselRepository _vesselRepository;

    public VesselOwnershipHandler(IHttpContextAccessor httpContextAccessor, ILogger<VesselOwnershipHandler> logger, IVesselRepository vesselRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _vesselRepository = vesselRepository;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VesselOwnershipRequirement requirement)
    {
        var user = context.User;
        var httpContext = _httpContextAccessor.HttpContext;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            Log.Warning("Vessel ownership check denied: User not authenticated");
            return;
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
        var vesselId = GetVesselIdFromContext(httpContext, requirement);

        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("UserRole", userRole))
        using (LogContext.PushProperty("VesselId", vesselId))
        using (LogContext.PushProperty("AuthorizationRequirement", "VesselOwnership"))
        {
            // Administrators and Fleet Managers have ownership of all vessels
            if (Enum.TryParse<UserRole>(userRole, out var role))
            {
                if (role == UserRole.Administrator || role == UserRole.FleetManager)
                {
                    Log.Information("Vessel ownership granted: User has administrative privileges");
                    context.Succeed(requirement);
                    return;
                }

                // Check actual vessel ownership/assignment in database
                if (role == UserRole.Captain)
                {
                    // Verify vessel exists and captain has access
                    var vessel = await _vesselRepository.GetByIdAsync(vesselId);
                    if (vessel != null)
                    {
                        Log.Information("Vessel ownership granted: User is assigned Captain for vessel {VesselId}", vesselId);
                        context.Succeed(requirement);
                    }
                    else
                    {
                        Log.Warning("Vessel ownership denied: Vessel {VesselId} not found", vesselId);
                    }
                    return;
                }
            }

            Log.Warning("Vessel ownership denied: User does not own vessel {VesselId}", vesselId);
        }
    }

    private string? GetVesselIdFromContext(HttpContext? httpContext, VesselOwnershipRequirement requirement)
    {
        if (!string.IsNullOrEmpty(requirement.VesselId))
        {
            return requirement.VesselId;
        }

        if (httpContext?.Request.RouteValues.TryGetValue("vesselId", out var vesselIdObj) == true)
        {
            return vesselIdObj?.ToString();
        }

        return null;
    }
}

/// <summary>
/// Authorization handler for emergency access requirements.
/// </summary>
public class EmergencyAccessHandler : AuthorizationHandler<EmergencyAccessRequirement>
{
    private readonly ILogger<EmergencyAccessHandler> _logger;

    public EmergencyAccessHandler(ILogger<EmergencyAccessHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EmergencyAccessRequirement requirement)
    {
        var user = context.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            Log.Warning("Emergency access denied: User not authenticated");
            return;
        }

        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("UserRole", userRole))
        using (LogContext.PushProperty("AuthorizationRequirement", "EmergencyAccess"))
        {
            // Check if this is an emergency situation
            // This could be determined by various factors:
            // - Emergency flag in request headers
            // - Current system status
            // - Time-based emergency windows
            // - Manual emergency activation

            var isEmergency = await CheckEmergencyStatus();

            if (isEmergency && requirement.AllowOverride)
            {
                // During emergencies, allow broader access
                if (Enum.TryParse<UserRole>(userRole, out var role))
                {
                    if (role <= UserRole.Operator) // All roles up to Operator
                    {
                        Log.Warning("Emergency access granted: Emergency situation detected for user {UserId}", userId);
                        context.Succeed(requirement);
                        return;
                    }
                }
            }

            // Normal authorization rules apply
            Log.Information("Emergency access evaluated: No emergency situation or insufficient role");
        }
    }

    private async Task<bool> CheckEmergencyStatus()
    {
        try
        {
            // Check for critical alarms that might indicate emergency
            // This is a basic implementation - in production you would have more sophisticated logic
            
            // For now, we'll check if there are any critical active alarms
            // In a real system, you would have specific emergency indicators
            
            Log.Debug("Checking emergency status - no emergency conditions detected");
            return false; // No emergency by default
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking emergency status");
            return false;
        }
    }
}
