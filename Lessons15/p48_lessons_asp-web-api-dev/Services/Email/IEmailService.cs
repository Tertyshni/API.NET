namespace p48_lessons_asp.Services.Email;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string recipient, string subject, string message);
}
