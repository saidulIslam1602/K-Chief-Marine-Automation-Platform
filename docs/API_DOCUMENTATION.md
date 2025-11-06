# API Documentation Guide

## Overview

The K-Chief Marine Automation Platform API provides comprehensive RESTful endpoints for managing marine vessel automation systems. This guide covers authentication, endpoints, request/response formats, error handling, and best practices.

## Base URL

```
Production: https://api.kchief.com
Development: https://dev-api.kchief.com
Local: http://localhost:5000
```

## API Versioning

The API uses URL-based versioning:
- Current version: `v1`
- Base path: `/api/v1/` (or `/api/` for backward compatibility)

## Authentication

The API supports two authentication methods:

### 1. JWT Bearer Token

**Obtaining a Token:**

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "operator",
  "password": "your-password"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-12-31T23:59:59Z",
  "user": {
    "id": "user-001",
    "username": "operator",
    "roles": ["Operator"]
  }
}
```

**Using the Token:**

Include the token in the `Authorization` header:

```http
GET /api/vessels
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 2. API Key

Include your API key in the `X-API-Key` header:

```http
GET /api/vessels
X-API-Key: your-api-key-here
```

**Obtaining an API Key:**

Contact support at support@kchief.com to obtain an API key.

### Authentication Errors

| Status Code | Description |
|------------|-------------|
| 401 | Missing or invalid authentication credentials |
| 403 | Valid credentials but insufficient permissions |

## Rate Limiting

API requests are rate-limited to ensure fair usage:

- **Default Limit**: 100 requests per minute per IP address
- **Per-Endpoint Limits**: Some endpoints may have specific limits
- **Headers**: Rate limit information is included in response headers:
  - `X-RateLimit-Limit`: Maximum requests allowed
  - `X-RateLimit-Remaining`: Remaining requests in current window
  - `X-RateLimit-Reset`: Time when the rate limit resets

**Rate Limit Exceeded Response:**

```json
{
  "type": "https://tools.ietf.org/html/rfc6585#section-4",
  "title": "Too Many Requests",
  "status": 429,
  "detail": "Rate limit exceeded. Please try again later.",
  "retryAfter": 60
}
```

## Request Format

### Headers

All requests should include:

```http
Content-Type: application/json
Accept: application/json
Authorization: Bearer {token}
# OR
X-API-Key: {api-key}
```

### Request Body

For POST, PUT, and PATCH requests, include a JSON body:

```json
{
  "field1": "value1",
  "field2": "value2"
}
```

## Response Format

### Success Responses

**200 OK:**
```json
{
  "id": "vessel-001",
  "name": "MV Atlantic",
  "status": "InService"
}
```

**201 Created:**
```json
{
  "id": "vessel-002",
  "name": "MV Pacific",
  "status": "InService",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

**204 No Content:**
Empty response body for successful DELETE operations.

### Error Responses

All error responses follow the [RFC 7807 Problem Details](https://tools.ietf.org/html/rfc7807) format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "The request contains invalid data.",
  "instance": "/api/vessels",
  "errors": {
    "field1": ["Field1 is required", "Field1 must be at least 3 characters"],
    "field2": ["Field2 must be a valid email address"]
  }
}
```

## Error Codes

| Status Code | Description | Common Causes |
|------------|-------------|---------------|
| 400 | Bad Request | Invalid request data, validation errors |
| 401 | Unauthorized | Missing or invalid authentication |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource doesn't exist |
| 409 | Conflict | Resource conflict (e.g., duplicate) |
| 422 | Unprocessable Entity | Semantic errors in request |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server error |
| 503 | Service Unavailable | Service temporarily unavailable |

## Endpoints

### Vessels

#### Get All Vessels

```http
GET /api/vessels
```

**Response:**
```json
[
  {
    "id": "vessel-001",
    "name": "MV Atlantic",
    "imoNumber": "IMO1234567",
    "callSign": "ATL1",
    "type": "Cargo",
    "length": 200.5,
    "width": 32.0,
    "draft": 12.5,
    "status": "InService"
  }
]
```

#### Get Vessel by ID

```http
GET /api/vessels/{id}
```

**Parameters:**
- `id` (path, required): Vessel identifier

**Response:**
```json
{
  "id": "vessel-001",
  "name": "MV Atlantic",
  "imoNumber": "IMO1234567",
  "callSign": "ATL1",
  "type": "Cargo",
  "length": 200.5,
  "width": 32.0,
  "draft": 12.5,
  "grossTonnage": 15000.0,
  "flag": "US",
  "status": "InService"
}
```

#### Create Vessel

```http
POST /api/vessels
Content-Type: application/json

{
  "name": "MV Pacific",
  "imoNumber": "IMO7654321",
  "callSign": "PAC1",
  "type": "Tanker",
  "length": 250.0,
  "width": 40.0,
  "draft": 15.0,
  "grossTonnage": 30000.0,
  "flag": "US"
}
```

**Response:** 201 Created

#### Update Vessel

```http
PUT /api/vessels/{id}
Content-Type: application/json

{
  "name": "MV Pacific Updated",
  "status": "OutOfService"
}
```

**Response:** 200 OK

#### Delete Vessel

```http
DELETE /api/vessels/{id}
```

**Response:** 204 No Content

### Engines

#### Get All Engines

```http
GET /api/engines
```

#### Get Engine by ID

```http
GET /api/engines/{id}
```

#### Start Engine

```http
POST /api/engines/{id}/start
```

**Response:**
```json
{
  "success": true,
  "message": "Engine started successfully",
  "engineId": "engine-001",
  "status": "Running"
}
```

#### Stop Engine

```http
POST /api/engines/{id}/stop
```

#### Set RPM

```http
POST /api/engines/{id}/rpm
Content-Type: application/json

{
  "rpm": 1500
}
```

### Alarms

#### Get All Alarms

```http
GET /api/alarms
```

#### Get Active Alarms

```http
GET /api/alarms/active
```

#### Acknowledge Alarm

```http
POST /api/alarms/{id}/acknowledge
Content-Type: application/json

{
  "acknowledgedBy": "operator-001"
}
```

### Alarm Rules

#### Get All Rules

```http
GET /api/alarmrules
```

#### Create Rule

```http
POST /api/alarmrules
Content-Type: application/json

{
  "name": "High Temperature Alert",
  "description": "Alert when temperature exceeds 100°C",
  "ruleType": "Threshold",
  "sourceType": "Sensor",
  "thresholdValue": 100.0,
  "thresholdOperator": "GreaterThan",
  "severity": "Warning"
}
```

## Pagination

List endpoints support pagination:

```http
GET /api/vessels?page=1&pageSize=20
```

**Query Parameters:**
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 20, max: 100)

**Response Headers:**
- `X-Pagination-Page`: Current page
- `X-Pagination-PageSize`: Page size
- `X-Pagination-TotalCount`: Total items
- `X-Pagination-TotalPages`: Total pages

## Filtering and Sorting

### Filtering

```http
GET /api/vessels?status=InService&type=Cargo
```

### Sorting

```http
GET /api/vessels?sortBy=name&sortOrder=asc
```

**Parameters:**
- `sortBy`: Field to sort by
- `sortOrder`: `asc` or `desc` (default: `asc`)

## Webhooks

The API supports webhooks for real-time notifications:

### Register Webhook

```http
POST /api/webhooks
Content-Type: application/json

{
  "url": "https://your-server.com/webhook",
  "events": ["vessel.created", "alarm.triggered"],
  "secret": "your-webhook-secret"
}
```

### Webhook Payload

```json
{
  "event": "vessel.created",
  "timestamp": "2024-01-15T10:30:00Z",
  "data": {
    "id": "vessel-001",
    "name": "MV Atlantic"
  }
}
```

## Best Practices

### 1. Use Appropriate HTTP Methods

- `GET`: Retrieve resources
- `POST`: Create resources
- `PUT`: Update entire resource
- `PATCH`: Partial update
- `DELETE`: Delete resources

### 2. Handle Errors Gracefully

Always check response status codes and handle errors appropriately:

```javascript
if (response.status === 429) {
  // Rate limit exceeded - retry after delay
  await delay(response.headers['retry-after']);
}
```

### 3. Use Pagination

For large datasets, always use pagination:

```http
GET /api/vessels?page=1&pageSize=20
```

### 4. Cache Responses

Cache GET responses when appropriate to reduce API calls.

### 5. Respect Rate Limits

Implement exponential backoff when rate limits are exceeded.

### 6. Use Correlation IDs

Include correlation IDs in requests for tracking:

```http
X-Correlation-ID: abc123def456
```

## SDKs and Libraries

### .NET

```csharp
var client = new KChiefApiClient("https://api.kchief.com", "your-api-key");
var vessels = await client.Vessels.GetAllAsync();
```

### JavaScript/TypeScript

```typescript
import { KChiefClient } from '@kchief/api-client';

const client = new KChiefClient({
  baseUrl: 'https://api.kchief.com',
  apiKey: 'your-api-key'
});

const vessels = await client.vessels.getAll();
```

## Support

For API support:
- **Email**: support@kchief.com
- **Documentation**: https://docs.kchief.com
- **Status Page**: https://status.kchief.com

## Related Documentation

- [API Changelog](API_CHANGELOG.md)
- [Authentication Guide](AUTHENTICATION.md)
- [Error Reference](ERROR_REFERENCE.md)
- [Swagger UI](https://api.kchief.com/swagger)

