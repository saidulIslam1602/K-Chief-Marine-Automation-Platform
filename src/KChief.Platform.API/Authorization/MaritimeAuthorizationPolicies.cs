using Microsoft.AspNetCore.Authorization;
using KChief.Platform.Core.Models;

namespace KChief.Platform.API.Authorization;

/// <summary>
/// Defines authorization policies for the K-Chief Marine Automation Platform.
/// </summary>
public static class MaritimeAuthorizationPolicies
{
    // Policy names
    public const string RequireAdministrator = "RequireAdministrator";
    public const string RequireFleetManager = "RequireFleetManager";
    public const string RequireCaptain = "RequireCaptain";
    public const string RequireChiefEngineer = "RequireChiefEngineer";
    public const string RequireNavigationOfficer = "RequireNavigationOfficer";
    public const string RequireEngineOperator = "RequireEngineOperator";
    public const string RequireOperator = "RequireOperator";
    public const string RequireObserver = "RequireObserver";
    public const string RequireMaintenance = "RequireMaintenance";
    public const string RequireShoreSupport = "RequireShoreSupport";
    public const string RequireGuest = "RequireGuest";

    // Permission-based policies
    public const string RequireFullAccess = "RequireFullAccess";
    public const string RequireFleetManagement = "RequireFleetManagement";
    public const string RequireVesselControl = "RequireVesselControl";
    public const string RequireEngineControl = "RequireEngineControl";
    public const string RequireNavigation = "RequireNavigation";
    public const string RequireRouteManagement = "RequireRouteManagement";
    public const string RequireMaintenanceAccess = "RequireMaintenanceAccess";
    public const string RequireDiagnostics = "RequireDiagnostics";
    public const string RequireRemoteMonitoring = "RequireRemoteMonitoring";
    public const string RequireBasicControl = "RequireBasicControl";
    public const string RequireReadOnly = "RequireReadOnly";

    // Vessel-specific policies
    public const string RequireVesselAccess = "RequireVesselAccess";
    public const string RequireVesselOwnership = "RequireVesselOwnership";
    public const string RequireEmergencyAccess = "RequireEmergencyAccess";

    /// <summary>
    /// Configures all authorization policies.
    /// </summary>
    public static void ConfigurePolicies(AuthorizationOptions options)
    {
        // Role-based policies
        options.AddPolicy(RequireAdministrator, policy =>
            policy.RequireRole(UserRole.Administrator.ToString()));

        options.AddPolicy(RequireFleetManager, policy =>
            policy.RequireRole(
                UserRole.Administrator.ToString(),
                UserRole.FleetManager.ToString()));

        options.AddPolicy(RequireCaptain, policy =>
            policy.RequireRole(
                UserRole.Administrator.ToString(),
                UserRole.FleetManager.ToString(),
                UserRole.Captain.ToString()));

        options.AddPolicy(RequireChiefEngineer, policy =>
            policy.RequireRole(
                UserRole.Administrator.ToString(),
                UserRole.FleetManager.ToString(),
                UserRole.Captain.ToString(),
                UserRole.ChiefEngineer.ToString()));

        options.AddPolicy(RequireNavigationOfficer, policy =>
            policy.RequireRole(
                UserRole.Administrator.ToString(),
                UserRole.FleetManager.ToString(),
                UserRole.Captain.ToString(),
                UserRole.NavigationOfficer.ToString()));

        options.AddPolicy(RequireEngineOperator, policy =>
            policy.RequireRole(
                UserRole.Administrator.ToString(),
                UserRole.FleetManager.ToString(),
                UserRole.Captain.ToString(),
                UserRole.ChiefEngineer.ToString(),
                UserRole.EngineOperator.ToString()));

        options.AddPolicy(RequireOperator, policy =>
            policy.RequireRole(
                UserRole.Administrator.ToString(),
                UserRole.FleetManager.ToString(),
                UserRole.Captain.ToString(),
                UserRole.ChiefEngineer.ToString(),
                UserRole.NavigationOfficer.ToString(),
                UserRole.EngineOperator.ToString(),
                UserRole.Operator.ToString()));

        options.AddPolicy(RequireObserver, policy =>
            policy.RequireRole(
                UserRole.Administrator.ToString(),
                UserRole.FleetManager.ToString(),
                UserRole.Captain.ToString(),
                UserRole.ChiefEngineer.ToString(),
                UserRole.NavigationOfficer.ToString(),
                UserRole.EngineOperator.ToString(),
                UserRole.Operator.ToString(),
                UserRole.Observer.ToString()));

        options.AddPolicy(RequireMaintenance, policy =>
            policy.RequireRole(
                UserRole.Administrator.ToString(),
                UserRole.FleetManager.ToString(),
                UserRole.Captain.ToString(),
                UserRole.ChiefEngineer.ToString(),
                UserRole.Maintenance.ToString()));

        options.AddPolicy(RequireShoreSupport, policy =>
            policy.RequireRole(
                UserRole.Administrator.ToString(),
                UserRole.FleetManager.ToString(),
                UserRole.ShoreSupport.ToString()));

        options.AddPolicy(RequireGuest, policy =>
            policy.RequireAuthenticatedUser());

        // Permission-based policies
        options.AddPolicy(RequireFullAccess, policy =>
            policy.RequireClaim("Permission", "FullAccess"));

        options.AddPolicy(RequireFleetManagement, policy =>
            policy.RequireClaim("Permission", "FullAccess", "FleetManagement"));

        options.AddPolicy(RequireVesselControl, policy =>
            policy.RequireClaim("Permission", "FullAccess", "FleetManagement", "VesselControl"));

        options.AddPolicy(RequireEngineControl, policy =>
            policy.RequireClaim("Permission", "FullAccess", "FleetManagement", "VesselControl", "EngineControl"));

        options.AddPolicy(RequireNavigation, policy =>
            policy.RequireClaim("Permission", "FullAccess", "FleetManagement", "VesselControl", "Navigation"));

        options.AddPolicy(RequireRouteManagement, policy =>
            policy.RequireClaim("Permission", "FullAccess", "FleetManagement", "VesselControl", "Navigation", "RouteManagement"));

        options.AddPolicy(RequireMaintenanceAccess, policy =>
            policy.RequireClaim("Permission", "FullAccess", "Maintenance"));

        options.AddPolicy(RequireDiagnostics, policy =>
            policy.RequireClaim("Permission", "FullAccess", "Maintenance", "Diagnostics"));

        options.AddPolicy(RequireRemoteMonitoring, policy =>
            policy.RequireClaim("Permission", "FullAccess", "FleetManagement", "RemoteMonitoring"));

        options.AddPolicy(RequireBasicControl, policy =>
            policy.RequireClaim("Permission", "FullAccess", "FleetManagement", "VesselControl", "EngineControl", "Navigation", "BasicControl"));

        options.AddPolicy(RequireReadOnly, policy =>
            policy.RequireAuthenticatedUser());

        // Custom requirement-based policies
        options.AddPolicy(RequireVesselAccess, policy =>
            policy.Requirements.Add(new VesselAccessRequirement()));

        options.AddPolicy(RequireVesselOwnership, policy =>
            policy.Requirements.Add(new VesselOwnershipRequirement()));

        options.AddPolicy(RequireEmergencyAccess, policy =>
            policy.Requirements.Add(new EmergencyAccessRequirement()));
    }
}

/// <summary>
/// Requirement for vessel access authorization.
/// </summary>
public class VesselAccessRequirement : IAuthorizationRequirement
{
    public string? VesselId { get; set; }
    public bool RequireOwnership { get; set; } = false;
}

/// <summary>
/// Requirement for vessel ownership authorization.
/// </summary>
public class VesselOwnershipRequirement : IAuthorizationRequirement
{
    public string? VesselId { get; set; }
}

/// <summary>
/// Requirement for emergency access authorization.
/// </summary>
public class EmergencyAccessRequirement : IAuthorizationRequirement
{
    public bool AllowOverride { get; set; } = true;
    public TimeSpan MaxDuration { get; set; } = TimeSpan.FromHours(1);
}
