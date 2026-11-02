using Application.Interfaces;
using Microsoft.Extensions.Logging;
namespace Application.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled(cancellationToken);
        }
        _logger.LogInformation(
            "Mock Email Sent -> To: {ToEmail} | Subject: {Subject} | Body: {Body}",
            toEmail, subject, body);
        return Task.CompletedTask;
    }

    public async Task SendResetPasswordEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default)
    {
        string subject = "Rivendosja e fjalëkalimit";
        string body = $"Përshëndetje, përdorni këtë link ose token për të rivendosur fjalëkalimin: {resetLink}";
        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }

    public async Task SendEmailChangeConfirmationAsync(string toEmail, string confirmationLink, CancellationToken cancellationToken = default)
    {
        string subject = "Konfirmimi i Email-it të ri";
        string body = $"Përshëndetje, përdorni këtë link ose token për të konfirmuar email-in e ri: {confirmationLink}";
        await SendEmailAsync(toEmail, subject, body, cancellationToken);
    }
}