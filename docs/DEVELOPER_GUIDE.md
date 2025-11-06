# Developer Guide

## Getting Started

### Development Environment Setup

1. Install .NET 8 SDK
2. Install Visual Studio 2022 or VS Code
3. Install Git
4. Clone the repository

### Project Structure

```
HMI-Marine-Automation-Platform/
├── src/
│   ├── HMI.Platform.Core/      # Domain models and interfaces
│   ├── HMI.Platform.API/        # REST API
│   ├── HMI.VesselControl/       # Vessel control logic
│   ├── HMI.AlarmSystem/         # Alarm management
│   ├── HMI.DataAccess/          # Data access and message bus
│   ├── HMI.Protocols.OPC/       # OPC UA integration
│   └── HMI.Protocols.Modbus/    # Modbus integration
├── tests/
│   ├── HMI.Platform.Tests/      # Unit tests
│   └── HMI.Integration.Tests/   # Integration tests
└── docs/                           # Documentation
```

## Coding Standards

### C# Conventions

- Use PascalCase for public members
- Use camelCase for private fields
- Use async/await for I/O operations
- Use meaningful variable names
- Keep methods focused (single responsibility)

### Code Organization

- One class per file
- Namespace matches folder structure
- Group related functionality
- Use regions sparingly

### Documentation

- XML comments for public APIs
- README files for complex modules
- Inline comments for non-obvious logic

## Adding New Features

### 1. Create Feature Branch
```bash
git checkout -b feature/new-feature-name
```

### 2. Implement Feature
- Follow SOLID principles
- Write unit tests
- Update documentation

### 3. Run Tests
```bash
dotnet test
```

### 4. Create Pull Request
- Describe changes
- Link related issues
- Request code review

## Testing Guidelines

### Unit Tests
- Test one thing per test
- Use descriptive test names
- Arrange-Act-Assert pattern
- Mock external dependencies

### Integration Tests
- Test API endpoints
- Use WebApplicationFactory
- Clean up test data

### Test Coverage
- Aim for 80%+ coverage
- Focus on business logic
- Don't test framework code

## Debugging

### Local Debugging
1. Set breakpoints in IDE
2. Run in debug mode
3. Use debugger tools

### Remote Debugging
1. Attach debugger to process
2. Use logging for production issues
3. Use Application Insights

## Performance Considerations

- Use async/await for I/O
- Avoid blocking calls
- Cache frequently accessed data
- Optimize database queries
- Use connection pooling

## Security Best Practices

- Validate all input
- Use parameterized queries
- Sanitize output
- Implement rate limiting
- Use HTTPS in production
- Keep dependencies updated

## Git Workflow

1. Create feature branch from `main`
2. Make small, focused commits
3. Write descriptive commit messages
4. Push frequently
5. Create PR when ready
6. Address review feedback
7. Merge after approval

## Common Tasks

### Adding a New API Endpoint

1. Create controller method
2. Add route attribute
3. Add XML documentation
4. Write integration test
5. Update API documentation

### Adding a New Service

1. Create interface in Core
2. Implement service
3. Register in Program.cs
4. Write unit tests
5. Update documentation

### Adding a New Protocol

1. Create protocol project
2. Implement client interface
3. Add to dependency injection
4. Write integration tests
5. Document usage

## Resources

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [C# Coding Conventions](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [xUnit Documentation](https://xunit.net/)

