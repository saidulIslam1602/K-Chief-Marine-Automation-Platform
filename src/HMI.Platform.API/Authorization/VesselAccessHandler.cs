using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Serilog;
using Serilog.Context;
using HMI.Platform.Core.Models;

namespace HMI.Platform.API.Authorization;

/// <summary>
/// Authorization handler for vessel access requirements.
/// </summary>
public class VesselAccessHandler : AuthorizationHandler<VesselAccessRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<VesselAccessHandler> _logger;

    public VesselAccessHandler(IHttpContextAccessor httpContextAccessor, ILogger<VesselAccessHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VesselAccessRequirement requirement)
    {
        var user = context.User;
        var httpContext = _httpContextAccessor.HttpContext;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            Log.Warning("Vessel access denied: User not authenticated");
            return Task.CompletedTask;
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
                    return Task.CompletedTask;
                }

                // Captains have access to their assigned vessels
                if (role == UserRole.Captain)
                {
                    // TODO: Check vessel assignment in database
                    // For now, allow access to all vessels for captains
                    Log.Information("Vessel access granted: User is a Captain");
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }

                // Other roles have limited access based on their permissions
                if (HasVesselPermission(user, vesselId, role))
                {
                    Log.Information("Vessel access granted: User has required permissions");
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            Log.Warning("Vessel access denied: Insufficient permissions for vessel {VesselId}", vesselId);
        }

        return Task.CompletedTask;
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

    public VesselOwnershipHandler(IHttpContextAccessor httpContextAccessor, ILogger<VesselOwnershipHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VesselOwnershipRequirement requirement)
    {
        var user = context.User;
        var httpContext = _httpContextAccessor.HttpContext;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            Log.Warning("Vessel ownership check denied: User not authenticated");
            return Task.CompletedTask;
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
                    return Task.CompletedTask;
                }

                // TODO: Check actual vessel ownership/assignment in database
                // For now, only allow Captains to have ownership
                if (role == UserRole.Captain)
                {
                    Log.Information("Vessel ownership granted: User is assigned Captain");
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            Log.Warning("Vessel ownership denied: User does not own vessel {VesselId}", vesselId);
        }

        return Task.CompletedTask;
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

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EmergencyAccessRequirement requirement)
    {
        var user = context.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            Log.Warning("Emergency access denied: User not authenticated");
            return Task.CompletedTask;
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

            var isEmergency = CheckEmergencyStatus();

            if (isEmergency && requirement.AllowOverride)
            {
                // During emergencies, allow broader access
                if (Enum.TryParse<UserRole>(userRole, out var role))
                {
                    if (role <= UserRole.Operator) // All roles up to Operator
                    {
                        Log.Warning("Emergency access granted: Emergency situation detected for user {UserId}", userId);
                        context.Succeed(requirement);
                        return Task.CompletedTask;
                    }
                }
            }

            // Normal authorization rules apply
            Log.Information("Emergency access evaluated: No emergency situation or insufficient role");
        }

        return Task.CompletedTask;
    }

    private bool CheckEmergencyStatus()
    {
        // TODO: Implement actual emergency status checking
        // This could check:
        // - System alarms
        // - Emergency flags in database
        // - External emergency systems
        // - Manual emergency activation
        
        return false; // No emergency by default
    }
}
