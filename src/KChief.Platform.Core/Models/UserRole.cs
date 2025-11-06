namespace KChief.Platform.Core.Models;

/// <summary>
/// Defines user roles in the K-Chief Marine Automation Platform.
/// These roles are based on typical maritime operational hierarchy.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// System administrator with full access to all features and settings.
    /// </summary>
    Administrator = 0,

    /// <summary>
    /// Fleet manager with access to multiple vessels and high-level operations.
    /// </summary>
    FleetManager = 1,

    /// <summary>
    /// Ship captain with full control over assigned vessel operations.
    /// </summary>
    Captain = 2,

    /// <summary>
    /// Chief engineer with access to engine and technical systems.
    /// </summary>
    ChiefEngineer = 3,

    /// <summary>
    /// Navigation officer with access to navigation and route planning.
    /// </summary>
    NavigationOfficer = 4,

    /// <summary>
    /// Engine operator with access to engine controls and monitoring.
    /// </summary>
    EngineOperator = 5,

    /// <summary>
    /// General operator with basic vessel monitoring and control access.
    /// </summary>
    Operator = 6,

    /// <summary>
    /// Observer with read-only access to vessel data and systems.
    /// </summary>
    Observer = 7,

    /// <summary>
    /// Maintenance personnel with access to maintenance and diagnostic features.
    /// </summary>
    Maintenance = 8,

    /// <summary>
    /// Shore-based support with remote monitoring capabilities.
    /// </summary>
    ShoreSupport = 9,

    /// <summary>
    /// Guest user with very limited read-only access.
    /// </summary>
    Guest = 10
}
