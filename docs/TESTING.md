# Advanced Testing Patterns

## Overview

The K-Chief Marine Automation Platform includes comprehensive testing infrastructure with advanced patterns including test fixtures, builders, property-based testing, performance testing, contract testing, and test data generation.

## Testing Architecture

### Test Projects

```
tests/
├── KChief.Platform.Tests/          # Unit tests
│   ├── TestHelpers/
│   │   ├── TestFixtures/          # Test fixtures
│   │   ├── Builders/              # Test data builders
│   │   ├── PropertyBased/        # Property-based testing
│   │   ├── Performance/           # Performance testing
│   │   ├── Contract/              # Contract testing
│   │   └── DataGenerators/        # Test data generators
│   └── Examples/                  # Example tests
└── KChief.Integration.Tests/      # Integration tests
```

## Test Fixtures

### WebApplicationFixture

The `WebApplicationFixture` provides a test web application with in-memory database:

```csharp
public class MyControllerTests : IClassFixture<WebApplicationFixture>
{
    private readonly WebApplicationFixture _fixture;
    private readonly HttpClient _client;

    public MyControllerTests(WebApplicationFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetVessels_Should_Return_Success()
    {
        var response = await _client.GetAsync("/api/vessels");
        response.EnsureSuccessStatusCode();
    }
}
```

### Custom Fixtures

Create custom fixtures for specific test scenarios:

```csharp
public class DatabaseFixture : IAsyncLifetime
{
    public ApplicationDbContext DbContext { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        DbContext = new ApplicationDbContext(options);
        await DbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.DisposeAsync();
    }
}
```

## Test Data Builders

### Builder Pattern

Use builders to create test data with a fluent API:

```csharp
// Simple usage
var vessel = VesselBuilder.CreateDefault().Build();

// Custom vessel
var vessel = VesselBuilder.Create()
    .WithId("vessel-999")
    .WithName("Custom Vessel")
    .AsTanker()
    .OutOfService()
    .WithDimensions(200.0, 30.0, 10.0)
    .Build();

// Engine builder
var engine = EngineBuilder.Create()
    .WithVesselId("vessel-001")
    .WithMaxRpm(1500.0)
    .Running()
    .Build();

// Alarm builder
var alarm = AlarmBuilder.Create()
    .WithTitle("High Temperature")
    .AsCritical()
    .Acknowledged("operator-001")
    .Build();
```

### Available Builders

- **VesselBuilder**: Creates vessel instances
- **EngineBuilder**: Creates engine instances
- **AlarmBuilder**: Creates alarm instances

### Builder Methods

#### VesselBuilder

```csharp
VesselBuilder.Create()
    .WithId(string)
    .WithName(string)
    .WithImoNumber(string)
    .WithCallSign(string)
    .WithType(VesselType)
    .WithDimensions(double length, double width, double draft)
    .WithGrossTonnage(double)
    .WithFlag(string)
    .WithBuiltDate(DateTime)
    .WithOwner(string)
    .WithStatus(VesselStatus)
    .AsCargoVessel()
    .AsTanker()
    .AsContainerShip()
    .InService()
    .OutOfService()
    .Build()
```

#### EngineBuilder

```csharp
EngineBuilder.Create()
    .WithId(string)
    .WithVesselId(string)
    .WithName(string)
    .WithType(EngineType)
    .WithMaxRpm(double)
    .WithCurrentRpm(double)
    .WithStatus(EngineStatus)
    .WithTemperature(double)
    .WithPressure(double)
    .AsDiesel()
    .AsGasTurbine()
    .Running()
    .Stopped()
    .Overheated()
    .Build()
```

#### AlarmBuilder

```csharp
AlarmBuilder.Create()
    .WithId(string)
    .WithTitle(string)
    .WithDescription(string)
    .WithSeverity(AlarmSeverity)
    .WithStatus(AlarmStatus)
    .WithVesselId(string)
    .WithEngineId(string)
    .WithSensorId(string)
    .AsWarning()
    .AsCritical()
    .AsInfo()
    .Active()
    .Acknowledged(string user, DateTime?)
    .Cleared(string user, DateTime?)
    .Build()
```

## Property-Based Testing

### FsCheck Integration

Property-based testing uses FsCheck to generate random test data:

```csharp
using FsCheck.Xunit;

public class VesselPropertyTests : PropertyBasedTestBase
{
    [Property]
    public Property VesselId_Should_Be_Valid_Format(ValidVesselId vesselId)
    {
        return (vesselId.StartsWith("vessel-") && 
                vesselId.Length > 7).ToProperty();
    }

    [Property]
    public Property Vessel_Length_Should_Be_Positive(PositiveDouble length)
    {
        var vessel = new Vessel { Length = length };
        return (vessel.Length > 0).ToProperty();
    }
}
```

### Custom Generators

Create custom generators for domain-specific types:

```csharp
public static class VesselGenerators
{
    public static Arbitrary<string> ValidVesselId()
    {
        return Gen.Choose(1, 999)
            .Select(n => $"vessel-{n:D3}")
            .ToArbitrary();
    }

    public static Arbitrary<string> ValidImoNumber()
    {
        return Gen.Choose(1000000, 9999999)
            .Select(n => $"IMO{n}")
            .ToArbitrary();
    }
}
```

### Available Generators

- `ValidVesselId`: Generates vessel IDs (vessel-001, vessel-002, etc.)
- `ValidEngineId`: Generates engine IDs
- `ValidSensorId`: Generates sensor IDs
- `ValidImoNumber`: Generates IMO numbers
- `ValidCallSign`: Generates call signs
- `PositiveDouble`: Generates positive doubles
- `ValidRpm`: Generates valid RPM values (0-2000)
- `ValidTemperature`: Generates valid temperatures (-50 to 200)
- `ValidDate`: Generates valid dates
- `VesselTypeGen`: Generates vessel types
- `EngineTypeGen`: Generates engine types
- `AlarmSeverityGen`: Generates alarm severities

## Performance Testing

### PerformanceTestBase

Base class for performance tests with timing assertions:

```csharp
public class MyPerformanceTests : PerformanceTestBase
{
    [Fact]
    public void Operation_Should_Complete_Within_100ms()
    {
        AssertExecutionTime(() =>
        {
            // Perform operation
        }, TimeSpan.FromMilliseconds(100), "Operation");
    }

    [Fact]
    public void LoadTest_Should_Handle_1000_Items()
    {
        RunLoadTest(() =>
        {
            // Perform operation
        }, iterations: 100, maxTotalDuration: TimeSpan.FromSeconds(5));
    }
}
```

### Available Methods

- `AssertExecutionTime`: Asserts operation completes within time limit
- `AssertExecutionTimeAsync`: Async version
- `MeasureExecutionTime`: Measures and returns duration
- `MeasureExecutionTimeAsync`: Async version
- `RunLoadTest`: Runs multiple iterations
- `RunLoadTestAsync`: Async version

### Load Testing with NBomber

For more advanced load testing, use NBomber:

```csharp
using NBomber.CSharp;

var scenario = Scenario.Create("load_test", async context =>
{
    // Perform operation
    await Task.Delay(100);
    return Response.Ok();
})
.WithLoadSimulations(
    Simulation.InjectPerSec(rate: 100, during: TimeSpan.FromMinutes(1))
);

NBomberRunner
    .RegisterScenarios(scenario)
    .Run();
```

## Contract Testing

### ContractTestBase

Base class for contract tests:

```csharp
public class MyContractTests : ContractTestBase
{
    [Fact]
    public void Contract_Should_Have_Required_Properties()
    {
        var contract = new MyContract { /* ... */ };
        AssertContractHasRequiredProperties(contract, 
            "id", "name", "type");
    }

    [Fact]
    public void Contract_Should_Serialize_Correctly()
    {
        var contract = new MyContract { /* ... */ };
        AssertContractRoundTrip(contract);
    }

    [Fact]
    public void Contract_Should_Match_Schema()
    {
        var actual = new MyContract { /* ... */ };
        var expected = new MyContract { /* ... */ };
        AssertContractMatches(actual, expected, "MyContract");
    }
}
```

### Available Methods

- `AssertContractHasRequiredProperties`: Validates required properties exist
- `AssertContractRoundTrip`: Validates serialization/deserialization
- `AssertContractMatches`: Validates contract matches expected
- `AssertContractVersion`: Validates version compatibility
- `ValidateContractSchema`: Custom validation

## Test Data Generation

### Bogus Integration

Use Bogus for generating realistic test data:

```csharp
using KChief.Platform.Tests.TestHelpers.DataGenerators;

var faker = new Faker();

// Generate single instance
var vessel = faker.GenerateVessel();
var engine = faker.GenerateEngine(vesselId: "vessel-001");
var alarm = faker.GenerateAlarm(vesselId: "vessel-001");

// Generate collections
var vessels = faker.GenerateVessels(count: 100);
var engines = faker.GenerateEngines(count: 50, vesselId: "vessel-001");
var alarms = faker.GenerateAlarms(count: 200, vesselId: "vessel-001");
```

### Custom Data Generators

Extend FakerExtensions for custom generators:

```csharp
public static class FakerExtensions
{
    public static MyType GenerateMyType(this Faker faker)
    {
        return new MyType
        {
            Id = faker.Random.Guid().ToString(),
            Name = faker.Company.CompanyName(),
            // ... other properties
        };
    }
}
```

## Best Practices

### 1. Use Builders for Test Data

```csharp
// Good
var vessel = VesselBuilder.CreateDefault().Build();

// Avoid
var vessel = new Vessel
{
    Id = "vessel-001",
    Name = "Test Vessel",
    // ... many properties
};
```

### 2. Use Fixtures for Shared Setup

```csharp
// Good
public class MyTests : IClassFixture<DatabaseFixture>
{
    // Use fixture
}

// Avoid
[Fact]
public void Test()
{
    // Setup database in each test
}
```

### 3. Use Property-Based Testing for Invariants

```csharp
// Good - tests invariant for all valid inputs
[Property]
public Property Length_Should_Be_Positive(PositiveDouble length)
{
    return (length > 0).ToProperty();
}

// Avoid - only tests specific cases
[Fact]
public void Length_Should_Be_Positive()
{
    Assert.True(100.0 > 0);
}
```

### 4. Use Performance Tests for Critical Paths

```csharp
// Good - ensures performance requirements
[Fact]
public void CriticalOperation_Should_Complete_Within_100ms()
{
    AssertExecutionTime(() => DoCriticalOperation(), 
        TimeSpan.FromMilliseconds(100));
}
```

### 5. Use Contract Tests for API Stability

```csharp
// Good - ensures API contract doesn't break
[Fact]
public void Vessel_Contract_Should_Have_Required_Properties()
{
    AssertContractHasRequiredProperties(vessel, "id", "name");
}
```

### 6. Use Data Generators for Large Datasets

```csharp
// Good - generates realistic test data
var vessels = faker.GenerateVessels(count: 1000);

// Avoid - manual creation of large datasets
var vessels = new List<Vessel>();
for (int i = 0; i < 1000; i++)
{
    vessels.Add(new Vessel { /* ... */ });
}
```

## Running Tests

### Run All Tests

```bash
dotnet test
```

### Run Specific Test Project

```bash
dotnet test tests/KChief.Platform.Tests
```

### Run with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Performance Tests Only

```bash
dotnet test --filter "FullyQualifiedName~Performance"
```

### Run Property-Based Tests Only

```bash
dotnet test --filter "FullyQualifiedName~Property"
```

## Test Organization

### Structure

```
Tests/
├── UnitTests/
│   ├── Models/
│   ├── Services/
│   └── Controllers/
├── IntegrationTests/
│   ├── Controllers/
│   └── EndToEnd/
├── PerformanceTests/
│   └── LoadTests/
└── ContractTests/
    └── ApiContracts/
```

### Naming Conventions

- Test classes: `{ClassUnderTest}Tests`
- Test methods: `{Method}_Should_{ExpectedBehavior}`
- Fixtures: `{Purpose}Fixture`
- Builders: `{Type}Builder`

## Related Documentation

- [Architecture Documentation](ARCHITECTURE.md)
- [Developer Guide](DEVELOPER_GUIDE.md)
- [CI/CD Documentation](CI_CD.md)

