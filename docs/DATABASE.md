# Database Implementation Guide

## Overview

The K-Chief Marine Automation Platform uses Entity Framework Core with SQLite for data persistence. The implementation follows the Repository and Unit of Work patterns to provide a clean separation between the data access layer and business logic.

## Architecture

### Database Layer Components

1. **ApplicationDbContext** - Entity Framework DbContext
2. **Repository Pattern** - Generic and specific repositories
3. **Unit of Work Pattern** - Transaction management
4. **Database Migrations** - Schema versioning
5. **Data Seeding** - Initial data population

### Entity Framework Configuration

```csharp
// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### Connection Strings

**Development (SQLite):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=KChiefMarineAutomationPlatform_Dev.db"
  }
}
```

**Production (SQL Server):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=KChiefMarineAutomationPlatform;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true"
  }
}
```

## Repository Pattern

### Generic Repository Interface

```csharp
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(object id);
    Task<T> AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> filter);
    Task<int> CountAsync(Expression<Func<T, bool>>? filter = null);
}
```

### Specific Repository Interfaces

```csharp
public interface IVesselRepository : IRepository<Vessel>
{
    Task<IEnumerable<Vessel>> GetByStatusAsync(string status);
    Task<IEnumerable<Vessel>> GetByTypeAsync(string type);
    Task<Vessel?> GetVesselWithEnginesAsync(string vesselId);
}
```

## Unit of Work Pattern

### Interface

```csharp
public interface IUnitOfWork : IDisposable
{
    IVesselRepository Vessels { get; }
    IEngineRepository Engines { get; }
    ISensorRepository Sensors { get; }
    IAlarmRepository Alarms { get; }
    IMessageBusEventRepository MessageBusEvents { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

### Usage Example

```csharp
public class DatabaseVesselControlService : IVesselControlService
{
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<bool> StartEngineAsync(string vesselId, string engineId)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            
            var engine = await _unitOfWork.Engines.GetByIdAsync(engineId);
            if (engine == null) return false;
            
            engine.Status = EngineStatus.Running;
            engine.IsRunning = true;
            
            _unitOfWork.Engines.Update(engine);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            return false;
        }
    }
}
```

## Database Migrations

### Creating Migrations

```bash
# Create a new migration
dotnet ef migrations add MigrationName --project src/KChief.DataAccess

# Apply migrations to database
dotnet ef database update --project src/KChief.DataAccess

# Remove last migration
dotnet ef migrations remove --project src/KChief.DataAccess
```

### Migration Commands from API Project

```bash
cd src/KChief.Platform.API

# Create migration
dotnet ef migrations add MigrationName --project ../KChief.DataAccess/KChief.DataAccess.csproj

# Update database
dotnet ef database update --project ../KChief.DataAccess/KChief.DataAccess.csproj
```

## Data Seeding

### Seed Data Configuration

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // Configure entities...
    
    // Seed initial data
    SeedData(modelBuilder);
}

private static void SeedData(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Vessel>().HasData(
        new Vessel
        {
            Id = "vessel-001",
            Name = "MV Atlantic Explorer",
            Type = "Container Ship",
            Status = VesselStatus.Online,
            Location = "Port of Rotterdam",
            Length = 300.0,
            Width = 45.0,
            MaxSpeed = 24.5,
            CreatedAt = DateTime.UtcNow
        }
    );
}
```

## Entity Configuration

### Vessel Entity

```csharp
modelBuilder.Entity<Vessel>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Id).HasMaxLength(50);
    entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
    entity.Property(e => e.Type).HasMaxLength(50);
    entity.Property(e => e.Location).HasMaxLength(200);
    entity.HasIndex(e => e.Name);
});
```

### Engine Entity with Foreign Key

```csharp
modelBuilder.Entity<Engine>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Id).HasMaxLength(50);
    entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
    entity.Property(e => e.VesselId).HasMaxLength(50);
    entity.HasIndex(e => e.VesselId);
    
    // Configure foreign key relationship
    entity.HasOne<Vessel>()
          .WithMany()
          .HasForeignKey(e => e.VesselId)
          .OnDelete(DeleteBehavior.Cascade);
});
```

## Service Registration

### Dependency Injection Setup

```csharp
// Register Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repositories and Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IVesselRepository, VesselRepository>();
builder.Services.AddScoped<IEngineRepository, EngineRepository>();
builder.Services.AddScoped<ISensorRepository, SensorRepository>();
builder.Services.AddScoped<IAlarmRepository, AlarmRepository>();
builder.Services.AddScoped<IMessageBusEventRepository, MessageBusEventRepository>();

// Register services
builder.Services.AddScoped<IVesselControlService, DatabaseVesselControlService>();
```

## Testing with In-Memory Database

### Test Configuration

```csharp
public class DatabaseVesselControlServiceTests
{
    private ApplicationDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
    
    [Fact]
    public async Task StartEngineAsync_ShouldStartEngine_WhenEngineExists()
    {
        // Arrange
        using var context = GetInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var service = new DatabaseVesselControlService(unitOfWork, logger);
        
        // Add test data
        await context.Vessels.AddAsync(new Vessel { Id = "vessel-1", Name = "Test Vessel" });
        await context.Engines.AddAsync(new Engine { Id = "engine-1", VesselId = "vessel-1", Status = EngineStatus.Stopped });
        await context.SaveChangesAsync();
        
        // Act
        var result = await service.StartEngineAsync("vessel-1", "engine-1");
        
        // Assert
        Assert.True(result);
        var engine = await context.Engines.FindAsync("engine-1");
        Assert.Equal(EngineStatus.Running, engine.Status);
    }
}
```

## Performance Considerations

### Query Optimization

```csharp
// Use async methods
var vessels = await _unitOfWork.Vessels.GetAllAsync();

// Use specific queries instead of loading all data
var activeVessels = await _unitOfWork.Vessels.GetByStatusAsync("Online");

// Use pagination for large datasets
var vessels = await _unitOfWork.Vessels.GetAsync(
    filter: v => v.Status == VesselStatus.Online,
    orderBy: q => q.OrderBy(v => v.Name),
    includeProperties: "Engines"
);
```

### Connection Management

```csharp
// Unit of Work handles connection lifecycle
using var unitOfWork = serviceProvider.GetService<IUnitOfWork>();
// Connection is automatically managed and disposed
```

## Database Schema

### Tables Created

1. **Vessels** - Vessel information
2. **Engines** - Engine data with foreign key to Vessels
3. **Sensors** - Sensor readings
4. **Alarms** - Alarm records
5. **MessageBusEvents** - Event audit trail

### Indexes

- Primary keys on all Id columns
- Foreign key indexes
- Performance indexes on frequently queried columns (Name, Status, Type)
- Timestamp indexes for time-based queries

## Switching Between Implementations

### Configuration-Based Service Selection

```csharp
// In Program.cs
if (builder.Configuration.GetValue<bool>("UseDatabase"))
{
    builder.Services.AddScoped<IVesselControlService, DatabaseVesselControlService>();
}
else
{
    builder.Services.AddScoped<IVesselControlService, VesselControlService>();
}
```

This allows switching between in-memory and database implementations based on configuration.

## Benefits of This Implementation

1. **Separation of Concerns** - Clear separation between data access and business logic
2. **Testability** - Easy to mock repositories for unit testing
3. **Transaction Management** - Proper transaction handling with Unit of Work
4. **Performance** - Optimized queries and connection management
5. **Maintainability** - Clean, organized code structure
6. **Flexibility** - Easy to switch between different data sources
7. **Production Ready** - Proper error handling and logging
