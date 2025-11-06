using System.Text.Json;
using Xunit;

namespace KChief.Platform.Tests.TestHelpers.Contract;

/// <summary>
/// Base class for contract testing.
/// </summary>
public abstract class ContractTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Asserts that a contract matches the expected schema.
    /// </summary>
    protected void AssertContractMatches<T>(T actual, T expected, string contractName)
    {
        var actualJson = JsonSerializer.Serialize(actual, JsonOptions);
        var expectedJson = JsonSerializer.Serialize(expected, JsonOptions);

        Assert.Equal(expectedJson, actualJson);
    }

    /// <summary>
    /// Asserts that a contract has required properties.
    /// </summary>
    protected void AssertContractHasRequiredProperties<T>(T contract, params string[] requiredProperties)
    {
        var json = JsonSerializer.Serialize(contract, JsonOptions);
        var document = JsonDocument.Parse(json);

        foreach (var property in requiredProperties)
        {
            Assert.True(
                document.RootElement.TryGetProperty(property, out _),
                $"Contract is missing required property: {property}");
        }
    }

    /// <summary>
    /// Asserts that a contract serializes and deserializes correctly.
    /// </summary>
    protected void AssertContractRoundTrip<T>(T original) where T : class
    {
        var json = JsonSerializer.Serialize(original, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<T>(json, JsonOptions);

        Assert.NotNull(deserialized);
        var originalJson = JsonSerializer.Serialize(original, JsonOptions);
        var deserializedJson = JsonSerializer.Serialize(deserialized, JsonOptions);
        Assert.Equal(originalJson, deserializedJson);
    }

    /// <summary>
    /// Asserts that a contract version is compatible.
    /// </summary>
    protected void AssertContractVersion(string actualVersion, string expectedVersion)
    {
        Assert.Equal(expectedVersion, actualVersion);
    }

    /// <summary>
    /// Validates a contract against a schema.
    /// </summary>
    protected void ValidateContractSchema<T>(T contract, Action<T> validator)
    {
        validator(contract);
    }
}

