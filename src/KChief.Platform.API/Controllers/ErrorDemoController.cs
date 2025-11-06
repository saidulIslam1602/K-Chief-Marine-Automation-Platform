using Microsoft.AspNetCore.Mvc;
using KChief.Platform.Core.Exceptions;
using KChief.Platform.API.Services;

namespace KChief.Platform.API.Controllers;

/// <summary>
/// Controller for demonstrating error handling capabilities.
/// This controller is for testing and demonstration purposes only.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = false)]
public class ErrorDemoController : ControllerBase
{
    private readonly ErrorLoggingService _errorLoggingService;
    private readonly ILogger<ErrorDemoController> _logger;

    public ErrorDemoController(ErrorLoggingService errorLoggingService, ILogger<ErrorDemoController> logger)
    {
        _errorLoggingService = errorLoggingService;
        _logger = logger;
    }

    /// <summary>
    /// Demonstrates vessel not found exception handling.
    /// </summary>
    [HttpGet("vessel-not-found/{vesselId}")]
    public async Task<IActionResult> DemoVesselNotFound(string vesselId)
    {
        await Task.Delay(10);
        throw new VesselNotFoundException(vesselId);
    }

    /// <summary>
    /// Demonstrates engine not found exception handling.
    /// </summary>
    [HttpGet("engine-not-found/{vesselId}/{engineId}")]
    public async Task<IActionResult> DemoEngineNotFound(string vesselId, string engineId)
    {
        await Task.Delay(10);
        throw new EngineNotFoundException(engineId, vesselId);
    }

    /// <summary>
    /// Demonstrates validation exception handling.
    /// </summary>
    [HttpPost("validation-error")]
    public async Task<IActionResult> DemoValidationError([FromBody] TestModel model)
    {
        await Task.Delay(10);
        
        var validationErrors = new Dictionary<string, string[]>();
        
        if (string.IsNullOrEmpty(model.Name))
            validationErrors["name"] = new[] { "Name is required." };
            
        if (model.Age < 0)
            validationErrors["age"] = new[] { "Age must be non-negative." };
            
        if (model.Age > 150)
            validationErrors["age"] = new[] { "Age must be realistic." };

        if (validationErrors.Count > 0)
            throw new ValidationException(validationErrors);

        return Ok(new { message = "Validation passed", model });
    }

    /// <summary>
    /// Demonstrates vessel operation exception handling.
    /// </summary>
    [HttpPost("operation-error/{vesselId}")]
    public async Task<IActionResult> DemoOperationError(string vesselId, [FromBody] OperationRequest request)
    {
        await Task.Delay(10);
        throw new VesselOperationException(vesselId, request.Operation, 
            $"Operation '{request.Operation}' failed due to system constraints.")
            .WithContext("RequestedValue", request.Value)
            .WithContext("MaxAllowedValue", 1000);
    }

    /// <summary>
    /// Demonstrates protocol exception handling.
    /// </summary>
    [HttpGet("protocol-error")]
    public async Task<IActionResult> DemoProtocolError()
    {
        await Task.Delay(10);
        throw new ProtocolException("OPC UA", "opc.tcp://localhost:4840", 
            "Connection timeout after 30 seconds");
    }

    /// <summary>
    /// Demonstrates generic exception handling.
    /// </summary>
    [HttpGet("generic-error")]
    public async Task<IActionResult> DemoGenericError()
    {
        await Task.Delay(10);
        throw new InvalidOperationException("This is a generic system error for testing purposes.");
    }

    /// <summary>
    /// Demonstrates argument validation.
    /// </summary>
    [HttpGet("argument-error")]
    public async Task<IActionResult> DemoArgumentError(string? requiredParam)
    {
        await Task.Delay(10);
        
        if (string.IsNullOrEmpty(requiredParam))
            throw new ArgumentNullException(nameof(requiredParam));
            
        if (requiredParam.Length < 3)
            throw new ArgumentException("Parameter must be at least 3 characters long.", nameof(requiredParam));

        return Ok(new { message = "Parameter is valid", value = requiredParam });
    }

    /// <summary>
    /// Demonstrates timeout exception handling.
    /// </summary>
    [HttpGet("timeout-error")]
    public async Task<IActionResult> DemoTimeoutError()
    {
        await Task.Delay(10);
        throw new TimeoutException("Operation timed out after 30 seconds.");
    }

    /// <summary>
    /// Demonstrates business error logging.
    /// </summary>
    [HttpPost("business-error")]
    public async Task<IActionResult> DemoBusinessError([FromBody] BusinessOperationRequest request)
    {
        await Task.Delay(10);
        
        _errorLoggingService.LogBusinessError(
            "ProcessPayment", 
            "Insufficient funds for transaction",
            new { 
                AccountId = request.AccountId, 
                RequestedAmount = request.Amount, 
                AvailableBalance = 150.00 
            });

        return BadRequest(new { 
            error = "Business operation failed", 
            details = "Insufficient funds" 
        });
    }

    /// <summary>
    /// Demonstrates security event logging.
    /// </summary>
    [HttpPost("security-event")]
    public async Task<IActionResult> DemoSecurityEvent()
    {
        await Task.Delay(10);
        
        _errorLoggingService.LogSecurityEvent(
            "UnauthorizedAccess", 
            "Attempt to access restricted resource without proper authentication",
            HttpContext);

        throw new UnauthorizedAccessException("Access denied to restricted resource.");
    }
}

/// <summary>
/// Test model for validation demonstration.
/// </summary>
public class TestModel
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? Email { get; set; }
}

/// <summary>
/// Operation request model.
/// </summary>
public class OperationRequest
{
    public string Operation { get; set; } = string.Empty;
    public int Value { get; set; }
}

/// <summary>
/// Business operation request model.
/// </summary>
public class BusinessOperationRequest
{
    public string AccountId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
