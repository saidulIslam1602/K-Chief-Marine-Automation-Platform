using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HMI.DataAccess.Data;
using HMI.Platform.API;

namespace HMI.Platform.Tests.TestHelpers.TestFixtures;

/// <summary>
/// Fixture for web application testing.
/// </summary>
public class WebApplicationFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private string? _databasePath;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            _databasePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite($"Data Source={_databasePath}");
            });

            // Ensure database is created
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        // Additional initialization if needed
        await Task.CompletedTask;
    }

    public new async Task DisposeAsync()
    {
        // Clean up database
        if (!string.IsNullOrEmpty(_databasePath) && File.Exists(_databasePath))
        {
            try
            {
                File.Delete(_databasePath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        await base.DisposeAsync();
    }
}

