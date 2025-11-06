# API Changelog

All notable changes to the HMI Marine Automation Platform API will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-01-15

### Added

#### Authentication
- JWT Bearer token authentication
- API Key authentication
- Role-based access control (RBAC)
- Policy-based authorization

#### Vessels
- `GET /api/vessels` - Get all vessels
- `GET /api/vessels/{id}` - Get vessel by ID
- `POST /api/vessels` - Create vessel
- `PUT /api/vessels/{id}` - Update vessel
- `DELETE /api/vessels/{id}` - Delete vessel
- Vessel filtering and sorting
- Pagination support

#### Engines
- `GET /api/engines` - Get all engines
- `GET /api/engines/{id}` - Get engine by ID
- `POST /api/engines/{id}/start` - Start engine
- `POST /api/engines/{id}/stop` - Stop engine
- `POST /api/engines/{id}/rpm` - Set engine RPM
- Real-time engine status updates

#### Sensors
- `GET /api/sensors` - Get all sensors
- `GET /api/sensors/{id}` - Get sensor by ID
- `GET /api/sensors/{id}/readings` - Get sensor readings
- Historical data querying

#### Alarms
- `GET /api/alarms` - Get all alarms
- `GET /api/alarms/active` - Get active alarms
- `GET /api/alarms/{id}` - Get alarm by ID
- `POST /api/alarms` - Create alarm
- `POST /api/alarms/{id}/acknowledge` - Acknowledge alarm
- `POST /api/alarms/{id}/clear` - Clear alarm

#### Alarm Rules
- `GET /api/alarmrules` - Get all alarm rules
- `GET /api/alarmrules/{id}` - Get rule by ID
- `POST /api/alarmrules` - Create alarm rule
- `PUT /api/alarmrules/{id}` - Update alarm rule
- `DELETE /api/alarmrules/{id}` - Delete alarm rule
- Rule-based alarm triggering
- Threshold-based alarms
- Alarm escalation
- Alarm grouping and correlation

#### Alarm Trends
- `GET /api/alarmtrends` - Get alarm trends
- `GET /api/alarmtrends/alarm/{alarmId}/history` - Get alarm history
- Trend analysis and reporting

#### Alarm Groups
- `GET /api/alarmgroups` - Get all alarm groups
- `GET /api/alarmgroups/{id}` - Get group by ID
- `POST /api/alarmgroups/{id}/acknowledge` - Acknowledge group

#### Real-time Updates
- SignalR hub at `/hubs/vessel`
- Real-time vessel status updates
- Real-time engine status updates
- Real-time alarm notifications

#### Health Checks
- `GET /health` - Overall health check
- `GET /health/ready` - Readiness probe
- `GET /health/live` - Liveness probe
- Health Checks UI at `/health-ui`

#### Monitoring
- `GET /metrics` - Performance metrics endpoint
- Application Insights integration
- Custom metrics collection
- Distributed tracing

#### Middleware
- Request validation middleware
- Rate limiting middleware
- Response time tracking
- Correlation ID tracking
- Request/response logging
- Global exception handling

### Changed

- Initial API release

### Deprecated

- None

### Removed

- None

### Security

- JWT token expiration and refresh
- API key rotation support
- Rate limiting to prevent abuse
- Request validation and sanitization
- CORS configuration

## [Unreleased]

### Planned

#### Version 1.1.0
- Webhook support for event notifications
- GraphQL endpoint
- Batch operations
- Advanced filtering with query syntax
- Export functionality (CSV, Excel, PDF)

#### Version 1.2.0
- Multi-tenant support
- Organization management
- User management endpoints
- Audit logging API
- Backup and restore operations

#### Version 2.0.0
- API versioning improvements
- Breaking changes (will be documented)
- New authentication methods
- Enhanced real-time capabilities

## Versioning

- **Major version** (X.0.0): Breaking changes
- **Minor version** (0.X.0): New features, backward compatible
- **Patch version** (0.0.X): Bug fixes, backward compatible

## Migration Guides

### From v0.x to v1.0

No migration needed - this is the initial release.

## Deprecation Policy

- Deprecated endpoints will be marked in documentation
- Deprecated endpoints will remain functional for at least 6 months
- Breaking changes will be announced 3 months in advance
- Migration guides will be provided for breaking changes

## Support

For questions about API changes:
- **Email**: support@kchief.com
- **Documentation**: https://docs.kchief.com
- **Status Page**: https://status.kchief.com

