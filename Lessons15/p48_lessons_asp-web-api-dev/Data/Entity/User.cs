using System;
using p48_lessons_asp.Data.Enum;

namespace p48_lessons_asp.Data.Entity;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? FirstName { get; set; }
    public string? LaststName { get; set; }
    public DateTime? BirthdayDate { get; set; }
    public string? Email { get; set; }
    public string Login { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Default;

    public bool IsEmailConfirmed { get; set; } = false;
    public string? EmailVerificationCode { get; set; }
    public DateTime? EmailVerificationCodeExpires { get; set; }
    public int EmailCodeAttempts { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public List<TokenJournal>? TokenJournals { get; set; }
}
