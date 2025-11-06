using System.Runtime.Serialization;

namespace KChief.Platform.Core.Exceptions;

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
[Serializable]
public class ValidationException : KChiefException
{
    public override string ErrorCode => "VALIDATION_ERROR";

    public Dictionary<string, string[]> ValidationErrors { get; }

    public ValidationException(string message) 
        : base(message)
    {
        ValidationErrors = new Dictionary<string, string[]>();
    }

    public ValidationException(Dictionary<string, string[]> validationErrors) 
        : base("One or more validation errors occurred.")
    {
        ValidationErrors = validationErrors;
        WithContext("ValidationErrors", validationErrors);
    }

    public ValidationException(string field, string error) 
        : base($"Validation failed for field '{field}': {error}")
    {
        ValidationErrors = new Dictionary<string, string[]>
        {
            [field] = new[] { error }
        };
        WithContext("ValidationErrors", ValidationErrors);
    }

    public ValidationException(string message, Exception innerException) 
        : base(message, innerException)
    {
        ValidationErrors = new Dictionary<string, string[]>();
    }

    protected ValidationException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
        ValidationErrors = (Dictionary<string, string[]>?)info.GetValue(nameof(ValidationErrors), typeof(Dictionary<string, string[]>)) 
                          ?? new Dictionary<string, string[]>();
    }

    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ValidationErrors), ValidationErrors);
    }
}
