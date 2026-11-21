using System;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using p48_lessons_asp.Data;

namespace p48_lessons_asp.Models.UserRequestModels;

public class CreateUserRequest
{
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    private readonly DataContext _db;

    public CreateUserValidator(DataContext db)
    {
        _db = db;

        RuleFor(m => m.Login)
            .NotEmpty()
            .WithMessage("Login is required")
            .Matches("^[a-z0-9_]+$")
            .WithMessage("Login can only contain lowercase letters, numbers, and underscores")
            .MustAsync(async (login, token) => string.IsNullOrEmpty(login) ? true : !await _db.Users
                .AnyAsync(u => u.Login == login, token))
            .WithMessage("Login already in use");
        
        RuleFor(m => m.Password).NotEmpty().WithMessage("Your password cannot be empty")
                     .MinimumLength(8).WithMessage("Your password length must be at least 8.")
                     .MaximumLength(16).WithMessage("Your password length must not exceed 16.")
                     .Matches(@"[A-Z]+").WithMessage("Your password must contain at least one uppercase letter.")
                     .Matches(@"[a-z]+").WithMessage("Your password must contain at least one lowercase letter.")
                     .Matches(@"[0-9]+").WithMessage("Your password must contain at least one number.");
        
    }
}