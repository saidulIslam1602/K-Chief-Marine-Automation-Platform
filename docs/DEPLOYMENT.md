# Deployment Guide

## Prerequisites

- .NET 8 SDK
- Docker Desktop (for containerized deployment)
- Git

## Local Development

### 1. Clone Repository
```bash
git clone https://github.com/saidulIslam1602/HMI-Marine-Automation-Platform.git
cd HMI-Marine-Automation-Platform
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Build Solution
```bash
dotnet build
```

### 4. Run Tests
```bash
dotnet test
```

### 5. Run API
```bash
cd src/HMI.Platform.API
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger: `https://localhost:5001/swagger`

## Docker Deployment

### Build Docker Image
```bash
docker build -f docker/Dockerfile -t kchief-api:latest .
```

### Run with Docker Compose
```bash
cd docker
docker-compose up -d
```

### View Logs
```bash
docker-compose logs -f
```

### Stop Container
```bash
docker-compose down
```

## Production Deployment

### Environment Variables

Set the following environment variables:

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__DefaultConnection=<database-connection-string>
RabbitMQ__HostName=<rabbitmq-host>
RabbitMQ__Port=5672
```

### Docker Production Deployment

1. Build production image:
```bash
docker build -f docker/Dockerfile -t kchief-api:prod --target final .
```

2. Run container:
```bash
docker run -d \
  --name kchief-api \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  kchief-api:prod
```

### Kubernetes Deployment

1. Create deployment:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: kchief-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: kchief-api
  template:
    metadata:
      labels:
        app: kchief-api
    spec:
      containers:
      - name: kchief-api
        image: kchief-api:latest
        ports:
        - containerPort: 8080
```

2. Create service:
```yaml
apiVersion: v1
kind: Service
metadata:
  name: kchief-api-service
spec:
  selector:
    app: kchief-api
  ports:
  - port: 80
    targetPort: 8080
  type: LoadBalancer
```

## Health Checks

### Basic Health Check
```bash
curl http://localhost:8080/health
```

### Detailed Health Check
```bash
curl http://localhost:8080/health/ready
```

## Monitoring

### Application Insights
Configure Application Insights for production monitoring:

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Logging
Logs are written to:
- Console (development)
- File (production)
- Application Insights (optional)

## Backup and Recovery

### Database Backup
If using a database, implement regular backups:
```bash
# Example PostgreSQL backup
pg_dump -U user -d kchief_db > backup.sql
```

### Configuration Backup
Backup application configuration and secrets regularly.

## Rollback Procedure

1. Identify previous working version
2. Revert code changes
3. Rebuild and redeploy
4. Verify functionality
5. Monitor for issues

## Troubleshooting

### Container Won't Start
- Check logs: `docker logs kchief-api`
- Verify environment variables
- Check port availability

### API Not Responding
- Verify service is running
- Check firewall rules
- Verify load balancer configuration

### Database Connection Issues
- Verify connection string
- Check network connectivity
- Verify database is accessible

