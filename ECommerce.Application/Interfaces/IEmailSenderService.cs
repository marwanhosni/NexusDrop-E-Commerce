namespace ECommerce.Application.Interfaces;

public interface IEmailSenderService
{
    Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
}
