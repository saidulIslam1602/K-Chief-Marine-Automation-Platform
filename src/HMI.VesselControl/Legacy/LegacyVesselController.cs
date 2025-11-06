using System;
using System.Collections.Generic;

namespace HMI.VesselControl.Legacy;

/// <summary>
/// LEGACY CODE EXAMPLE - This represents old, unmaintainable code.
/// This class demonstrates code that needs modernization.
/// 
/// Problems with this legacy code:
/// 1. Static methods with global state
/// 2. No dependency injection
/// 3. No error handling
/// 4. Tight coupling
/// 5. No unit tests possible
/// 6. Synchronous operations
/// 7. No logging
/// 8. Magic numbers and strings
/// </summary>
public static class LegacyVesselController
{
    // Global state - bad practice
    private static Dictionary<string, object> _vessels = new Dictionary<string, object>();
    private static bool _isInitialized = false;

    // Static initialization - no dependency injection
    public static void Initialize()
    {
        if (!_isInitialized)
        {
            _vessels.Add("vessel1", new { Name = "Old Vessel", Status = "Running" });
            _isInitialized = true;
        }
    }

    // Synchronous operation - blocks thread
    // No error handling
    // Returns object instead of strongly typed
    public static object? GetVessel(string id)
    {
        if (!_isInitialized)
        {
            Initialize();
        }

        if (_vessels.ContainsKey(id))
        {
            return _vessels[id];
        }

        return null; // No proper error handling
    }

    // Magic numbers and strings
    // No validation
    // No logging
    public static void SetEngineRpm(string vesselId, int rpm)
    {
        if (rpm > 1000) // Magic number
        {
            // Silent failure - no exception, no logging
            return;
        }

        // Direct manipulation - no abstraction
        // No transaction support
        // No validation
    }

    // Tight coupling - hard to test
    // No separation of concerns
    public static void ProcessVesselData(string data)
    {
        // Direct string manipulation
        // No parsing validation
        // No error handling
        var parts = data.Split(',');
        if (parts.Length > 0)
        {
            // No null checks
            // No type safety
        }
    }
}

