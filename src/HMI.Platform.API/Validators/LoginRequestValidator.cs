using FluentValidation;
using HMI.Platform.API.Controllers;
using HMI.Platform.Core.Validation;

namespace HMI.Platform.API.Validators;

/// <summary>
/// Validator for LoginRequest using shared validation framework.
/// </summary>
public class LoginRequestValidator : BaseValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        ValidateRequiredString(RuleFor(x => x.Username), "Username")
            .Length(3, 255)
            .WithMessage("Username must be between 3 and 255 characters")
            .WithErrorCode("USERNAME_LENGTH")
            .Matches(@"^[a-zA-Z0-9_\-\.]+$")
            .WithMessage("Username can only contain letters, numbers, underscores, hyphens, and dots")
            .WithErrorCode("LOGIN_USERNAME_INVALID_FORMAT")
            .When(x => !string.IsNullOrEmpty(x.Username));

        ValidateRequiredString(RuleFor(x => x.Password), "Password")
            .Length(6, 255)
            .WithMessage("Password must be between 6 and 255 characters")
            .WithErrorCode("PASSWORD_LENGTH");
    }
}

