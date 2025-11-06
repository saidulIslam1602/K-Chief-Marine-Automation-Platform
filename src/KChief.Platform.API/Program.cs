using Microsoft.EntityFrameworkCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;
using Serilog.Context;
using KChief.Platform.API.Hubs;
using KChief.Platform.API.Services;
using KChief.Platform.API.HealthChecks;
using KChief.Platform.API.Middleware;
using KChief.Platform.API.Filters;
using KChief.Platform.API.Authorization;
using KChief.Platform.Core.Interfaces;
using KChief.AlarmSystem.Services;
using KChief.DataAccess.Data;
using KChief.DataAccess.Interfaces;
using KChief.DataAccess.Services;
using KChief.DataAccess.Repositories;
using KChief.Protocols.Modbus.Services;
using KChief.Protocols.OPC.Services;
using KChief.VesselControl.Services;

namespace KChief.Platform.API;

public class Program
{
    public static void Main(string[] args)
    {
        // Configure Serilog early to capture startup logs
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        try
        {
            Log.Information("Starting K-Chief Marine Automation Platform");
            
            var builder = WebApplication.CreateBuilder(args);
            
            // Use Serilog for logging
            builder.Host.UseSerilog();

        // Add services to the container
        builder.Services.AddControllers(options =>
        {
            // Add global filters
            options.Filters.Add<ModelValidationFilter>();
            options.Filters.Add<OperationCancelledExceptionFilter>();
        });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "K-Chief Marine Automation Platform API",
                Version = "v1",
                Description = "RESTful API for marine vessel control and monitoring"
            });
        });

        // Add Entity Framework
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Add SignalR
        builder.Services.AddSignalR();

        // Add Application Insights
        builder.Services.AddApplicationInsightsTelemetry();

        // Add Performance Monitoring
        builder.Services.AddSingleton<PerformanceMonitoringService>();

            // Add Error Logging Service
            builder.Services.AddScoped<ErrorLoggingService>();

            // Add Authentication Services
            builder.Services.AddScoped<IKChiefAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<IUserService, UserService>();

            // Configure JWT Authentication
            var jwtSecret = builder.Configuration["Authentication:JWT:Secret"];
            var jwtIssuer = builder.Configuration["Authentication:JWT:Issuer"];
            var jwtAudience = builder.Configuration["Authentication:JWT:Audience"];

            if (!string.IsNullOrEmpty(jwtSecret))
            {
                var key = Encoding.ASCII.GetBytes(jwtSecret);

                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = builder.Configuration.GetValue<bool>("Authentication:Security:RequireHttpsMetadata", false);
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = !string.IsNullOrEmpty(jwtIssuer),
                        ValidIssuer = jwtIssuer,
                        ValidateAudience = !string.IsNullOrEmpty(jwtAudience),
                        ValidAudience = jwtAudience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    // Configure SignalR JWT authentication
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }
                            
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddScheme<ApiKeyAuthenticationSchemeOptions, ApiKeyAuthenticationSchemeHandler>(
                    ApiKeyAuthenticationSchemeOptions.DefaultScheme, 
                    options => { });
            }

            // Configure Authorization
            builder.Services.AddAuthorization(options =>
            {
                MaritimeAuthorizationPolicies.ConfigurePolicies(options);
            });

            // Register authorization handlers
            builder.Services.AddScoped<IAuthorizationHandler, VesselAccessHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, VesselOwnershipHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, EmergencyAccessHandler>();

        // Add Health Checks
        builder.Services.AddHealthChecks()
            // Basic health checks
            .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"))
            .AddDbContextCheck<ApplicationDbContext>("database")
            
            // Custom health checks for dependencies
            .AddCheck<OpcUaHealthCheck>("opcua")
            .AddCheck<ModbusHealthCheck>("modbus") 
            .AddCheck<MessageBusHealthCheck>("messagebus")
            .AddCheck<VesselControlHealthCheck>("vesselcontrol")
            .AddCheck<AlarmSystemHealthCheck>("alarmsystem")
            
            // Memory checks
            .AddCheck("memory", () => 
            {
                var allocatedBytes = GC.GetTotalMemory(false);
                var allocatedMB = allocatedBytes / 1024.0 / 1024.0;
                return allocatedMB < 1024 
                    ? Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy($"Memory usage: {allocatedMB:F2} MB")
                    : Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded($"High memory usage: {allocatedMB:F2} MB");
            });

        // Add Health Checks UI
        builder.Services.AddHealthChecksUI(options =>
        {
            options.SetEvaluationTimeInSeconds(30);
            options.MaximumHistoryEntriesPerEndpoint(50);
            options.AddHealthCheckEndpoint("K-Chief API", "/health");
        }).AddInMemoryStorage();

        // Register repositories and Unit of Work
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IVesselRepository, VesselRepository>();
        builder.Services.AddScoped<IEngineRepository, EngineRepository>();
        builder.Services.AddScoped<ISensorRepository, SensorRepository>();
        builder.Services.AddScoped<IAlarmRepository, AlarmRepository>();
        builder.Services.AddScoped<IMessageBusEventRepository, MessageBusEventRepository>();

        // Register application services
        builder.Services.AddScoped<IVesselControlService, VesselControlService>();
        builder.Services.AddSingleton<IAlarmService, AlarmService>();
        builder.Services.AddSingleton<IOPCUaClient, OPCUaClientService>();
        builder.Services.AddSingleton<IModbusClient, ModbusClientService>();
        builder.Services.AddSingleton<IMessageBus, MessageBusService>();
        builder.Services.AddSingleton<RealtimeUpdateService>();

        // Add CORS for development
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "K-Chief API v1");
            });
        }

        app.UseHttpsRedirection();
        app.UseCors();

        // Add correlation ID middleware (must be very early in pipeline)
        app.UseMiddleware<CorrelationIdMiddleware>();

        // Add request/response logging middleware
        app.UseMiddleware<RequestResponseLoggingMiddleware>();

        // Add global exception handling middleware (must be early in pipeline)
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

            // Add performance monitoring middleware
            app.UseMiddleware<PerformanceMonitoringMiddleware>();

            // Add authentication and authorization
            app.UseAuthentication();
            app.UseMiddleware<ApiKeyAuthenticationMiddleware>();
            app.UseAuthorization();
        app.MapControllers();

        // Map SignalR hub
        app.MapHub<VesselHub>("/hubs/vessel");

        // Map Health Check endpoints
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false, // Exclude all checks for liveness - just check if app is running
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        // Map Health Checks UI
        app.MapHealthChecksUI(options =>
        {
            options.UIPath = "/health-ui";
            options.ApiPath = "/health-api";
        });

        // Add performance metrics endpoint
        app.MapGet("/metrics", (PerformanceMonitoringService performanceService) =>
        {
            return Results.Ok(performanceService.GetPerformanceStats());
        }).WithTags("Monitoring");

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
