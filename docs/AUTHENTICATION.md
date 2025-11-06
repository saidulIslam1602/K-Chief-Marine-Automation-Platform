# Authentication Guide

## Overview

The HMI Marine Automation Platform API supports two authentication methods:
1. **JWT Bearer Token** - Recommended for user applications
2. **API Key** - Recommended for server-to-server communication

## JWT Bearer Token Authentication

### Obtaining a Token

Authenticate using the login endpoint:

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "operator",
  "password": "your-password"
}
```

**Success Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c2VyLTAwMSIsIm5hbWUiOiJvcGVyYXRvciIsInJvbGUiOiJPcGVyYXRvciIsImV4cCI6MTcwNTI4ODAwMH0...",
  "expiresAt": "2024-01-15T12:00:00Z",
  "refreshToken": "refresh-token-here",
  "user": {
    "id": "user-001",
    "username": "operator",
    "email": "operator@example.com",
    "roles": ["Operator"],
    "permissions": ["vessels.read", "vessels.write", "engines.control"]
  }
}
```

**Error Response (401 Unauthorized):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Invalid username or password"
}
```

### Using the Token

Include the token in the `Authorization` header:

```http
GET /api/vessels
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Token Expiration

- **Access Token**: Valid for 1 hour (default)
- **Refresh Token**: Valid for 7 days (default)

### Refreshing a Token

When a token expires, use the refresh token to obtain a new access token:

```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "refresh-token-here"
}
```

**Response:**
```json
{
  "token": "new-access-token",
  "expiresAt": "2024-01-15T13:00:00Z",
  "refreshToken": "new-refresh-token"
}
```

### Token Claims

JWT tokens contain the following claims:

- `sub`: User ID
- `name`: Username
- `email`: User email
- `role`: User role
- `permissions`: Array of permissions
- `exp`: Expiration time (Unix timestamp)
- `iat`: Issued at time (Unix timestamp)

### Example Token Decode

```json
{
  "sub": "user-001",
  "name": "operator",
  "email": "operator@example.com",
  "role": "Operator",
  "permissions": ["vessels.read", "vessels.write"],
  "exp": 1705288000,
  "iat": 1705284400
}
```

## API Key Authentication

### Obtaining an API Key

Contact support at support@kchief.com to obtain an API key.

### Using the API Key

Include the API key in the `X-API-Key` header:

```http
GET /api/vessels
X-API-Key: your-api-key-here
```

### API Key Format

API keys follow this format:
```
kchief_live_abc123def456ghi789jkl012mno345pqr678stu901vwx234yz
```

Prefixes:
- `kchief_live_`: Production API key
- `kchief_test_`: Test API key

### API Key Scopes

API keys can have different scopes:

- **Read-only**: Can only read data
- **Read-write**: Can read and write data
- **Full access**: Can perform all operations

### Rotating API Keys

1. Generate a new API key
2. Update your application to use the new key
3. Revoke the old key after verification

## Roles and Permissions

### Roles

- **Observer**: Read-only access
- **Operator**: Read and control access
- **Administrator**: Full access including user management

### Permissions

| Permission | Description |
|------------|-------------|
| `vessels.read` | Read vessel information |
| `vessels.write` | Create/update vessels |
| `vessels.delete` | Delete vessels |
| `engines.read` | Read engine information |
| `engines.control` | Control engines (start/stop/set RPM) |
| `alarms.read` | Read alarm information |
| `alarms.acknowledge` | Acknowledge alarms |
| `alarms.clear` | Clear alarms |
| `alarmrules.read` | Read alarm rules |
| `alarmrules.write` | Create/update alarm rules |
| `users.read` | Read user information |
| `users.write` | Create/update users |
| `users.delete` | Delete users |

## Authorization

### Role-Based Access Control (RBAC)

Endpoints are protected by role requirements:

```csharp
[Authorize(Roles = "Operator,Administrator")]
[HttpPost("engines/{id}/start")]
public async Task<ActionResult> StartEngine(string id)
{
    // Only Operators and Administrators can start engines
}
```

### Policy-Based Authorization

More granular control using policies:

```csharp
[Authorize(Policy = "RequireEngineControl")]
[HttpPost("engines/{id}/stop")]
public async Task<ActionResult> StopEngine(string id)
{
    // Requires specific permission
}
```

## Error Responses

### 401 Unauthorized

```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Authentication required. Please provide a valid token or API key."
}
```

**Common Causes:**
- Missing `Authorization` header
- Invalid or expired token
- Invalid API key

### 403 Forbidden

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "You do not have permission to perform this action."
}
```

**Common Causes:**
- Insufficient role
- Missing required permission
- API key with insufficient scope

## Best Practices

### 1. Store Tokens Securely

- Never commit tokens to version control
- Use environment variables or secure key stores
- Rotate tokens regularly

### 2. Handle Token Expiration

Implement token refresh logic:

```javascript
async function makeRequest(url, options = {}) {
  let response = await fetch(url, {
    ...options,
    headers: {
      ...options.headers,
      'Authorization': `Bearer ${accessToken}`
    }
  });

  if (response.status === 401) {
    // Token expired, refresh it
    await refreshToken();
    // Retry request
    response = await fetch(url, options);
  }

  return response;
}
```

### 3. Use HTTPS

Always use HTTPS in production to protect tokens and API keys.

### 4. Implement Retry Logic

Implement exponential backoff for authentication failures:

```javascript
async function retryWithBackoff(fn, maxRetries = 3) {
  for (let i = 0; i < maxRetries; i++) {
    try {
      return await fn();
    } catch (error) {
      if (i === maxRetries - 1) throw error;
      await delay(Math.pow(2, i) * 1000);
    }
  }
}
```

### 5. Monitor Authentication Failures

Monitor and alert on authentication failures to detect potential security issues.

## Security Considerations

### Token Security

- Tokens are signed using HMAC SHA256
- Tokens include expiration time
- Tokens should be transmitted over HTTPS only
- Tokens should be stored securely (not in localStorage for web apps)

### API Key Security

- API keys are long-lived and should be rotated regularly
- Never share API keys publicly
- Use different keys for different environments
- Revoke compromised keys immediately

### Rate Limiting

Authentication endpoints are rate-limited:
- Login: 5 attempts per minute per IP
- Token refresh: 10 requests per minute per IP

## Support

For authentication issues:
- **Email**: support@kchief.com
- **Documentation**: https://docs.kchief.com/authentication
- **Status Page**: https://status.kchief.com
