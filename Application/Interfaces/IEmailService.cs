namespace Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
    Task SendResetPasswordEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default);
    Task SendEmailChangeConfirmationAsync(string toEmail, string confirmationLink, CancellationToken cancellationToken = default);
}