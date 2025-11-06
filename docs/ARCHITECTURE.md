# HMI Marine Automation Platform - Architecture Documentation

## System Architecture Overview

The HMI Marine Automation Platform follows a modular, layered architecture designed for scalability, maintainability, and extensibility.

## Architecture Layers

### 1. Presentation Layer
- **HMI.Platform.API**: RESTful API endpoints
- **SignalR Hubs**: Real-time communication
- **Swagger/OpenAPI**: API documentation

### 2. Application Layer
- **HMI.VesselControl**: Vessel and engine control logic
- **HMI.AlarmSystem**: Alarm management
- **HMI.Platform.API.Services**: Application services

### 3. Domain Layer
- **HMI.Platform.Core**: Domain models, interfaces, and core business logic
  - Models: Vessel, Engine, Sensor, Alarm
  - Interfaces: IVesselControlService, IAlarmService, IOPCUaClient, IModbusClient, IMessageBus

### 4. Infrastructure Layer
- **HMI.DataAccess**: Data persistence and message bus
- **HMI.Protocols.OPC**: OPC UA protocol integration
- **HMI.Protocols.Modbus**: Modbus TCP/RTU integration

## Component Diagram

```
┌─────────────────────────────────────────────────────────┐
│                  Presentation Layer                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ REST API     │  │ SignalR Hub  │  │ Swagger UI   │  │
│  │ Controllers  │  │              │  │              │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                          │
┌─────────────────────────────────────────────────────────┐
│                  Application Layer                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ Vessel       │  │ Alarm        │  │ Realtime     │  │
│  │ Control      │  │ System       │  │ Updates      │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                          │
┌─────────────────────────────────────────────────────────┐
│                    Domain Layer                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ Models       │  │ Interfaces   │  │ Core Logic   │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                          │
┌─────────────────────────────────────────────────────────┐
│                Infrastructure Layer                      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ OPC UA       │  │ Modbus       │  │ Message Bus  │  │
│  │ Protocol     │  │ Protocol     │  │ (RabbitMQ)   │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
```

## Design Patterns

### 1. Dependency Injection
All services are registered in `Program.cs` using dependency injection for loose coupling and testability.

### 2. Repository Pattern
Data access is abstracted through interfaces, allowing for easy testing and future database implementations.

### 3. Service Layer Pattern
Business logic is encapsulated in service classes, keeping controllers thin.

### 4. Event-Driven Architecture
- SignalR for real-time updates
- Message bus for asynchronous communication
- Event handlers for alarm notifications

## Technology Stack

- **.NET 8**: Runtime and framework
- **ASP.NET Core**: Web API framework
- **SignalR**: Real-time communication
- **OPC UA**: Industrial communication protocol
- **Modbus**: Industrial communication protocol
- **RabbitMQ**: Message broker
- **Docker**: Containerization
- **xUnit**: Testing framework

## Data Flow

### Vessel Control Flow
1. Client sends HTTP request to API
2. Controller validates request
3. Service layer processes business logic
4. Real-time update sent via SignalR
5. Event published to message bus (optional)
6. Response returned to client

### Alarm Flow
1. Alarm condition detected
2. AlarmService creates alarm
3. AlarmCreated event raised
4. SignalR broadcasts to connected clients
5. Message bus publishes event (optional)
6. Alarm logged and persisted

## Scalability Considerations

- **Horizontal Scaling**: Stateless API design allows multiple instances
- **Message Bus**: Decouples services for independent scaling
- **Async Operations**: Non-blocking I/O for better throughput
- **Caching**: Can be added for frequently accessed data

## Security Considerations

- **Input Validation**: All endpoints validate input
- **Error Handling**: Proper exception handling without exposing internals
- **CORS**: Configured for development (should be restricted in production)
- **HTTPS**: Enforced in production
- **Authentication**: Ready for JWT/OAuth integration

## Deployment Architecture

```
┌─────────────┐
│   Client    │
│  (Browser)  │
└──────┬──────┘
       │ HTTPS/WebSocket
       │
┌──────▼──────────────────┐
│   Load Balancer         │
└──────┬──────────────────┘
       │
   ┌───┴───┬─────────┬─────────┐
   │       │         │         │
┌──▼──┐ ┌──▼──┐  ┌──▼──┐  ┌──▼──┐
│ API │ │ API │  │ API │  │ API │
│ Pod │ │ Pod │  │ Pod │  │ Pod │
└──┬──┘ └──┬──┘  └──┬──┘  └──┬──┘
   │       │         │         │
   └───┬───┴─────────┴─────────┘
       │
   ┌───▼──────────┐
   │  RabbitMQ    │
   │  Message Bus │
   └──────────────┘
```

## Future Enhancements

- Database persistence layer
- Authentication and authorization
- Rate limiting
- API versioning
- GraphQL endpoint
- gRPC support
- Kubernetes deployment
- Monitoring and observability (Prometheus, Grafana)

