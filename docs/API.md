# K-Chief Marine Automation Platform - API Documentation

## Base URL

- Development: `https://localhost:5001` or `http://localhost:5000`
- Production: `https://api.kchief.example.com`

## Authentication

Currently, the API does not require authentication for development. In production, JWT tokens or API keys should be used.

## API Endpoints

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
    "name": "MS Atlantic Explorer",
    "type": "Container Ship",
    "status": "Online",
    "engines": [...],
    "lastUpdated": "2025-11-06T14:30:00Z"
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
  "name": "MS Atlantic Explorer",
  "type": "Container Ship",
  "status": "Online",
  "engines": [...],
  "lastUpdated": "2025-11-06T14:30:00Z"
}
```

#### Get Vessel Engines
```http
GET /api/vessels/{id}/engines
```

#### Get Vessel Sensors
```http
GET /api/vessels/{id}/sensors
```

### Engines

#### Start Engine
```http
POST /api/engines/{vesselId}/engines/{engineId}/start
```

**Response:**
```json
{
  "message": "Engine start command accepted",
  "vesselId": "vessel-001",
  "engineId": "engine-001"
}
```

#### Stop Engine
```http
POST /api/engines/{vesselId}/engines/{engineId}/stop
```

#### Set Engine RPM
```http
POST /api/engines/{vesselId}/engines/{engineId}/rpm
Content-Type: application/json

{
  "rpm": 800
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

#### Create Alarm
```http
POST /api/alarms
Content-Type: application/json

{
  "title": "High Temperature",
  "description": "Engine temperature exceeds threshold",
  "severity": "Warning",
  "vesselId": "vessel-001",
  "engineId": "engine-001"
}
```

#### Acknowledge Alarm
```http
POST /api/alarms/{id}/acknowledge
Content-Type: application/json

{
  "acknowledgedBy": "operator-001"
}
```

#### Clear Alarm
```http
POST /api/alarms/{id}/clear
```

## SignalR Hub

### Connection
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/vessel")
    .build();
```

### Events

#### VesselUpdated
```javascript
connection.on("VesselUpdated", (vessel) => {
    console.log("Vessel updated:", vessel);
});
```

#### EngineUpdated
```javascript
connection.on("EngineUpdated", (vesselId, engine) => {
    console.log("Engine updated:", vesselId, engine);
});
```

#### AlarmCreated
```javascript
connection.on("AlarmCreated", (alarm) => {
    console.log("Alarm created:", alarm);
});
```

## Error Responses

### 400 Bad Request
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid request parameters"
}
```

### 404 Not Found
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Vessel with ID 'invalid-id' not found."
}
```

### 500 Internal Server Error
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An error occurred while processing your request."
}
```

## Rate Limiting

Rate limiting is not currently implemented but should be added for production:
- 100 requests per minute per IP
- 1000 requests per hour per API key

## Versioning

API versioning is planned for future releases. Current version: v1

## Swagger Documentation

Interactive API documentation is available at:
- Development: `https://localhost:5001/swagger`

