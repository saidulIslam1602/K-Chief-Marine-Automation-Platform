# Authentication and Authorization

## Overview

The K-Chief Marine Automation Platform implements comprehensive authentication and authorization using industry-standard practices including JWT tokens, role-based access control (RBAC), policy-based authorization, and API key authentication. This implementation demonstrates production-grade security practices essential for maritime automation systems.

## Architecture

### Security Stack

```
Client Request
    ↓
Authentication Middleware
├── JWT Bearer Authentication
├── API Key Authentication
└── OAuth 2.0 (Future)
    ↓
Authorization Policies
├── Role-Based Access Control
├── Policy-Based Authorization
├── Custom Requirements
└── Resource-Specific Permissions
    ↓
Protected Resources
```

### Key Components

1. **JWT Authentication** - Stateless token-based authentication
2. **Role-Based Authorization** - Maritime hierarchy-based access control
3. **Policy-Based Authorization** - Fine-grained permission system
4. **API Key Authentication** - Service-to-service communication
5. **Custom Authorization Handlers** - Vessel-specific and emergency access
6. **User Management** - Complete user lifecycle management

## Authentication Methods

### 1. JWT Bearer Authentication

JWT (JSON Web Token) authentication provides secure, stateless authentication for web and mobile clients.

#### Configuration

```json
{
  "Authentication": {
    "JWT": {
      "Secret": "your-super-secret-jwt-signing-key-that-is-at-least-256-bits-long-for-security",
      "Issuer": "KChief.Platform.API",
      "Audience": "KChief.Platform.API",
      "ExpirationMinutes": 60,
      "RefreshTokenExpirationDays": 7
    }
  }
}
```

#### Token Structure

```json
{
  "sub": "user-001",
  "name": "john.smith",
  "email": "john.smith@kchief.com",
  "role": "Captain",
  "FullName": "John Smith",
  "Department": "Navigation",
  "JobTitle": "Ship Captain",
  "Permission": ["VesselControl", "Navigation"],
  "iss": "KChief.Platform.API",
  "aud": "KChief.Platform.API",
  "exp": 1703123456,
  "iat": 1703119856
}
```

#### Usage Example

```bash
# Login to get JWT token
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "captain",
    "password": "captain123"
  }'

# Use JWT token in subsequent requests
curl -X GET "http://localhost:5000/api/vessels" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### 2. API Key Authentication

API key authentication enables secure service-to-service communication with scoped permissions.

#### API Key Format

```http
# Header-based authentication
X-API-Key: ak_1234567890abcdef1234567890abcdef

# Query parameter authentication
GET /api/vessels?apikey=ak_1234567890abcdef1234567890abcdef

# Authorization header authentication
Authorization: ApiKey ak_1234567890abcdef1234567890abcdef
```

#### API Key Properties

- **Scoped Permissions**: Limited to specific operations
- **IP Restrictions**: Optional IP address whitelisting
- **Rate Limiting**: Configurable request limits
- **Expiration**: Optional expiration dates
- **Usage Tracking**: Request count and last used timestamps

### 3. OAuth 2.0 Integration (Future Enhancement)

Placeholder for external identity providers like Google, Microsoft, and Azure AD.

## User Roles and Hierarchy

### Maritime Role Hierarchy

The platform implements a maritime-specific role hierarchy based on typical ship operations:

```
Administrator (0)           # Full system access
    ↓
FleetManager (1)           # Multiple vessel management
    ↓
Captain (2)                # Vessel command authority
    ↓
ChiefEngineer (3)         # Technical systems authority
NavigationOfficer (4)      # Navigation systems
    ↓
EngineOperator (5)        # Engine control systems
Operator (6)              # General operations
    ↓
Observer (7)              # Read-only monitoring
Maintenance (8)           # Maintenance systems
ShoreSupport (9)          # Remote monitoring
    ↓
Guest (10)                # Limited read-only access
```

### Role Permissions

#### Administrator
- **Permissions**: FullAccess
- **Capabilities**: Complete system control, user management, configuration
- **Access**: All vessels, all systems, all data

#### Fleet Manager
- **Permissions**: FleetManagement, VesselControl
- **Capabilities**: Multi-vessel operations, fleet-wide monitoring
- **Access**: All assigned vessels, operational data

#### Captain
- **Permissions**: VesselControl, Navigation
- **Capabilities**: Complete vessel command, navigation control
- **Access**: Assigned vessel(s), all vessel systems

#### Chief Engineer
- **Permissions**: EngineControl, Maintenance
- **Capabilities**: Engine systems, technical maintenance
- **Access**: Engine systems, maintenance data, diagnostics

#### Navigation Officer
- **Permissions**: Navigation, RouteManagement
- **Capabilities**: Navigation systems, route planning
- **Access**: Navigation systems, weather data, charts

#### Engine Operator
- **Permissions**: EngineControl
- **Capabilities**: Engine operation and monitoring
- **Access**: Engine controls, performance data

#### Operator
- **Permissions**: BasicControl
- **Capabilities**: General vessel operations
- **Access**: Basic controls, monitoring dashboards

#### Observer
- **Permissions**: ReadOnly
- **Capabilities**: System monitoring, data viewing
- **Access**: Read-only access to assigned systems

#### Maintenance
- **Permissions**: Maintenance, Diagnostics
- **Capabilities**: Maintenance operations, system diagnostics
- **Access**: Maintenance systems, diagnostic tools

#### Shore Support
- **Permissions**: RemoteMonitoring
- **Capabilities**: Remote vessel monitoring and support
- **Access**: Remote monitoring systems, communication

#### Guest
- **Permissions**: LimitedReadOnly
- **Capabilities**: Very limited system viewing
- **Access**: Public dashboards, basic vessel status

## Authorization Policies

### Role-Based Policies

```csharp
// Require specific role or higher
[Authorize(Policy = "RequireCaptain")]
public async Task<IActionResult> StartVessel(string vesselId) { }

// Require specific permission
[Authorize(Policy = "RequireVesselControl")]
public async Task<IActionResult> ControlEngine(string vesselId) { }
```

### Policy Definitions

#### Hierarchical Policies
- `RequireAdministrator`: Administrator only
- `RequireFleetManager`: Fleet Manager and above
- `RequireCaptain`: Captain and above
- `RequireChiefEngineer`: Chief Engineer and above (includes Captain+)
- `RequireOperator`: Operator and above

#### Permission-Based Policies
- `RequireFullAccess`: Full system access
- `RequireVesselControl`: Vessel operation permissions
- `RequireEngineControl`: Engine system permissions
- `RequireNavigation`: Navigation system permissions
- `RequireMaintenanceAccess`: Maintenance system permissions
- `RequireReadOnly`: Read-only access (all authenticated users)

#### Custom Requirement Policies
- `RequireVesselAccess`: Vessel-specific access control
- `RequireVesselOwnership`: Vessel ownership verification
- `RequireEmergencyAccess`: Emergency situation override

### Custom Authorization Requirements

#### Vessel Access Requirement

Validates user access to specific vessels based on:
- Administrative privileges (Admin, Fleet Manager)
- Vessel assignment (Captain, Officers)
- Role-based permissions (Operators, Observers)

```csharp
[Authorize(Policy = "RequireVesselAccess")]
public async Task<IActionResult> GetVesselDetails(string vesselId) { }
```

#### Emergency Access Requirement

Provides elevated access during emergency situations:
- Automatic detection of emergency conditions
- Time-limited access elevation
- Comprehensive audit logging
- Override capabilities for critical situations

```csharp
[Authorize(Policy = "RequireEmergencyAccess")]
public async Task<IActionResult> EmergencyShutdown(string vesselId) { }
```

## User Management

### User Model

```csharp
public class User
{
    public string Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public UserRole Role { get; set; }
    public string Department { get; set; }
    public string JobTitle { get; set; }
    public bool IsActive { get; set; }
    public bool EmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockedUntil { get; set; }
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsLocked => LockedUntil.HasValue && LockedUntil > DateTime.UtcNow;
}
```

### Default Users

The system includes pre-configured users for demonstration:

| Username | Password | Role | Department | Description |
|----------|----------|------|------------|-------------|
| admin | admin123 | Administrator | IT | System Administrator |
| captain | captain123 | Captain | Navigation | Ship Captain |
| engineer | engineer123 | ChiefEngineer | Engineering | Chief Engineer |
| operator | operator123 | Operator | Operations | Vessel Operator |
| observer | observer123 | Observer | Monitoring | System Observer |

### User Operations

#### Authentication Endpoints

```http
POST /api/auth/login
POST /api/auth/refresh
POST /api/auth/revoke
GET  /api/auth/me
POST /api/auth/change-password
```

#### User Management (Future Enhancement)

```http
GET    /api/users              # List users
POST   /api/users              # Create user
GET    /api/users/{id}         # Get user
PUT    /api/users/{id}         # Update user
DELETE /api/users/{id}         # Delete user
POST   /api/users/{id}/lock    # Lock user account
POST   /api/users/{id}/unlock  # Unlock user account
POST   /api/users/{id}/reset-password  # Reset password
```

## Security Features

### Password Security

- **Hashing**: BCrypt with salt rounds (configurable)
- **Complexity Requirements**: Configurable password policies
- **History**: Prevent password reuse (future enhancement)
- **Expiration**: Configurable password expiration (future enhancement)

### Account Security

- **Failed Login Protection**: Configurable attempt limits
- **Account Lockout**: Automatic lockout after failed attempts
- **Session Management**: JWT token expiration and refresh
- **Audit Logging**: Comprehensive authentication event logging

### Token Security

- **Secure Signing**: HMAC-SHA256 with configurable secrets
- **Short Expiration**: Configurable token lifetimes
- **Refresh Tokens**: Secure token renewal (future enhancement)
- **Token Revocation**: Blacklist capability (future enhancement)

### API Security

- **Rate Limiting**: Configurable request limits per API key
- **IP Restrictions**: Optional IP address whitelisting
- **Scope Limitations**: Fine-grained permission scoping
- **Usage Monitoring**: Request tracking and analytics

## Configuration

### Development Environment

```json
{
  "Authentication": {
    "JWT": {
      "Secret": "development-jwt-secret-key-for-testing-only-not-for-production-use",
      "ExpirationMinutes": 120
    },
    "Security": {
      "RequireHttpsMetadata": false,
      "MaxFailedLoginAttempts": 10,
      "AccountLockoutMinutes": 5,
      "PasswordRequirements": {
        "MinLength": 6,
        "RequireDigit": false,
        "RequireLowercase": false,
        "RequireUppercase": false,
        "RequireNonAlphanumeric": false
      }
    }
  }
}
```

### Production Environment

```json
{
  "Authentication": {
    "JWT": {
      "Secret": "REPLACE_WITH_SECURE_JWT_SECRET_IN_PRODUCTION",
      "ExpirationMinutes": 30
    },
    "ApiKey": {
      "RequireHttps": true
    },
    "Security": {
      "RequireHttpsMetadata": true,
      "MaxFailedLoginAttempts": 3,
      "AccountLockoutMinutes": 60,
      "PasswordRequirements": {
        "MinLength": 12,
        "RequireDigit": true,
        "RequireLowercase": true,
        "RequireUppercase": true,
        "RequireNonAlphanumeric": true
      }
    }
  }
}
```

## Implementation Examples

### Protecting Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for all actions
public class VesselsController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "RequireObserver")] // Read-only access
    public async Task<IActionResult> GetVessels() { }

    [HttpPost]
    [Authorize(Policy = "RequireFleetManager")] // Create vessels
    public async Task<IActionResult> CreateVessel() { }

    [HttpPut("{vesselId}")]
    [Authorize(Policy = "RequireVesselAccess")] // Vessel-specific access
    public async Task<IActionResult> UpdateVessel(string vesselId) { }

    [HttpPost("{vesselId}/start")]
    [Authorize(Policy = "RequireCaptain")] // Command authority required
    public async Task<IActionResult> StartVessel(string vesselId) { }

    [HttpPost("{vesselId}/emergency-stop")]
    [Authorize(Policy = "RequireEmergencyAccess")] // Emergency override
    public async Task<IActionResult> EmergencyStop(string vesselId) { }
}
```

### Custom Authorization Logic

```csharp
public class VesselAccessHandler : AuthorizationHandler<VesselAccessRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VesselAccessRequirement requirement)
    {
        var user = context.User;
        var vesselId = GetVesselIdFromContext();

        // Administrators and Fleet Managers have access to all vessels
        if (user.IsInRole("Administrator") || user.IsInRole("FleetManager"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // Check vessel assignment for other roles
        if (HasVesselAccess(user, vesselId))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

### Accessing User Information

```csharp
[HttpGet("profile")]
[Authorize]
public async Task<IActionResult> GetProfile()
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var username = User.FindFirst(ClaimTypes.Name)?.Value;
    var role = User.FindFirst(ClaimTypes.Role)?.Value;
    var department = User.FindFirst("Department")?.Value;
    var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();

    return Ok(new
    {
        UserId = userId,
        Username = username,
        Role = role,
        Department = department,
        Permissions = permissions
    });
}
```

## Testing Authentication

### Login Test

```bash
# Test user login
curl -X POST "http://localhost:5000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "captain",
    "password": "captain123"
  }'

# Expected response:
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64-encoded-refresh-token",
  "expiresAt": "2023-12-07T15:30:00Z",
  "user": {
    "id": "captain-001",
    "username": "captain",
    "email": "captain@kchief.com",
    "fullName": "John Smith",
    "role": "Captain",
    "department": "Navigation",
    "jobTitle": "Ship Captain"
  }
}
```

### Protected Resource Test

```bash
# Test protected endpoint without authentication
curl -X GET "http://localhost:5000/api/vessels"
# Expected: 401 Unauthorized

# Test protected endpoint with valid JWT
curl -X GET "http://localhost:5000/api/vessels" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
# Expected: 200 OK with vessel data

# Test insufficient permissions
curl -X POST "http://localhost:5000/api/vessels" \
  -H "Authorization: Bearer observer-jwt-token" \
  -H "Content-Type: application/json" \
  -d '{"name": "New Vessel"}'
# Expected: 403 Forbidden
```

### API Key Test

```bash
# Test API key authentication
curl -X GET "http://localhost:5000/api/vessels" \
  -H "X-API-Key: ak_1234567890abcdef1234567890abcdef"

# Test API key with query parameter
curl -X GET "http://localhost:5000/api/vessels?apikey=ak_1234567890abcdef1234567890abcdef"

# Test API key with Authorization header
curl -X GET "http://localhost:5000/api/vessels" \
  -H "Authorization: ApiKey ak_1234567890abcdef1234567890abcdef"
```

## Security Best Practices

### JWT Security

1. **Use Strong Secrets**: Minimum 256-bit signing keys
2. **Short Expiration**: Keep token lifetimes short (15-60 minutes)
3. **Secure Storage**: Store tokens securely on client side
4. **HTTPS Only**: Always use HTTPS in production
5. **Token Validation**: Validate all token claims

### Password Security

1. **Strong Hashing**: Use BCrypt with appropriate cost factor
2. **Password Policies**: Enforce complexity requirements
3. **Account Lockout**: Implement failed login protection
4. **Secure Transmission**: Always use HTTPS for authentication
5. **Regular Updates**: Encourage regular password changes

### API Key Security

1. **Scope Limitation**: Limit API key permissions to minimum required
2. **IP Restrictions**: Use IP whitelisting when possible
3. **Rate Limiting**: Implement appropriate request limits
4. **Regular Rotation**: Rotate API keys regularly
5. **Secure Storage**: Store API key hashes, not plain text

### General Security

1. **Principle of Least Privilege**: Grant minimum required permissions
2. **Defense in Depth**: Layer multiple security controls
3. **Audit Logging**: Log all authentication and authorization events
4. **Regular Reviews**: Periodically review user access and permissions
5. **Security Monitoring**: Monitor for suspicious authentication patterns

## Monitoring and Auditing

### Authentication Events

All authentication events are logged with structured data:

```json
{
  "timestamp": "2023-12-07T14:30:00Z",
  "level": "Information",
  "message": "User authentication successful",
  "properties": {
    "correlationId": "a1b2c3d4",
    "userId": "captain-001",
    "username": "captain",
    "authenticationMethod": "Password",
    "clientIP": "192.168.1.100",
    "userAgent": "Mozilla/5.0...",
    "success": true
  }
}
```

### Authorization Events

Authorization decisions are logged for audit purposes:

```json
{
  "timestamp": "2023-12-07T14:31:00Z",
  "level": "Warning",
  "message": "Authorization failed for vessel access",
  "properties": {
    "correlationId": "a1b2c3d4",
    "userId": "observer-001",
    "username": "observer",
    "resource": "/api/vessels/vessel-001/start",
    "policy": "RequireCaptain",
    "reason": "InsufficientRole"
  }
}
```

### Security Metrics

Key security metrics to monitor:

- **Failed Login Rate**: Monitor for brute force attacks
- **Account Lockouts**: Track locked accounts and patterns
- **Token Usage**: Monitor JWT token validation failures
- **Permission Denials**: Track authorization failures by resource
- **API Key Usage**: Monitor API key request patterns and failures

This comprehensive authentication and authorization system provides enterprise-grade security suitable for critical maritime automation systems while maintaining usability and flexibility for different operational scenarios.
