using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using p48_lessons_asp.Options;

namespace p48_lessons_asp.Services.Email;

public class GmailService: IEmailService, IDisposable
{
    private readonly GmailOptions _options;
    private ILogger<GmailService> _logger;
    private SmtpClient _smtpClient;
    private bool _disposed;

    public GmailService(IOptions<GmailOptions> options, ILogger<GmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _disposed = false;

        _smtpClient = new();
        try
        {
            _smtpClient.Connect(_options.Host, _options.Port, MailKit.Security.SecureSocketOptions.StartTls);
            _smtpClient.Authenticate(_options.SenderEmail, _options.Password);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Gmail service is fault");
        }
    }

    public void Dispose()
    {
        if(_disposed) return;
        try
        {
            if (_smtpClient.IsConnected)
            {
                _smtpClient.Disconnect(true);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Gmail service error while dispose SMTP Client");
        }
        finally
        {
            _smtpClient.Dispose();
            _disposed = true;
        }
        
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if(_disposed) return;
        try
        {
            if (_smtpClient.IsConnected)
            {
                await _smtpClient.DisconnectAsync(true);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Gmail service error while dispose SMTP Client");
        }
        finally
        {
            _smtpClient.Dispose();
            _disposed = true;
        }
        
        GC.SuppressFinalize(this);
    }

    public async Task<bool> SendEmailAsync(string recipient, string subject, string message)
    {
        if (!_smtpClient.IsConnected)
        {
            await _smtpClient.ConnectAsync(_options.Host, _options.Port, MailKit.Security.SecureSocketOptions.StartTls);
        }
        if (!_smtpClient.IsAuthenticated)
        {
            await _smtpClient.AuthenticateAsync(_options.SenderEmail, _options.Password);
        }
        using var mailMessage = new MimeMessage();
        mailMessage.From.Add(new MailboxAddress("P48Org", _options.SenderEmail));
        mailMessage.To.Add(new MailboxAddress("", recipient));
        mailMessage.Subject = subject;
        mailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html){Text = message};
        try
        {
            await _smtpClient.SendAsync(mailMessage);
            return true;
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Gmail Service is error while sending mail");
        }
        return false;
    }
}
