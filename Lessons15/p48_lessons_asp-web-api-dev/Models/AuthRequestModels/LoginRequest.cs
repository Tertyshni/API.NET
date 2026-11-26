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
            .WithMessage("Login can only contain lowercase letters, numbers, and underscores")
            .MinimumLength(3).WithMessage("Login must contain at least 3 characters")
            .MaximumLength(30).WithMessage("Login must not exceed 30 characters");
        
        RuleFor(m => m.Password).NotEmpty().WithMessage("Password is required")
            .MaximumLength(16).WithMessage("Password must not exceed 16 characters");
    }
}