using System;
using FluentValidation;

namespace p48_lessons_asp.Models.AuthRequestModels;

public class LoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginValidator : AbstractValidator<LoginRequest>
{

    public LoginValidator()
    {
        RuleFor(m => m.Login)
            .NotEmpty()
            .WithMessage("Login is required")
            .Matches("^[a-z0-9_]+$")
            .WithMessage("Login can only contain lowercase letters, numbers, and underscores");
        
        RuleFor(m => m.Password).NotEmpty().WithMessage("Password is required");
    }
}