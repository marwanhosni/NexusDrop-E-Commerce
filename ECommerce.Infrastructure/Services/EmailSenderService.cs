using ECommerce.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class EmailSenderService : IEmailSenderService
{
    private readonly ILogger<EmailSenderService> _logger;

    public EmailSenderService(ILogger<EmailSenderService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        _logger.LogInformation("================== [DISPATCH EMAIL] ==================");
        _logger.LogInformation("To: {ToEmail}", toEmail);
        _logger.LogInformation("Subject: {Subject}", subject);
        _logger.LogInformation("Content: {HtmlMessage}", htmlMessage);
        _logger.LogInformation("=====================================================");

        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var html = $@"
            <div style='background:#0b0f19; color:#f8fafc; padding:30px; font-family:sans-serif; border-radius:12px; border:1px solid #06b6d4;'>
                <h2 style='color:#06b6d4; margin-top:0;'>MARWAN HOSNY STORE // SECURITY DISPATCH</h2>
                <p>Hello operator, a password reset was requested for your account ({toEmail}).</p>
                <p>Click the link below to initialize your new access key:</p>
                <p style='margin:25px 0;'>
                    <a href='{resetLink}' style='background:linear-gradient(135deg, #06b6d4, #a855f7); color:#ffffff; padding:12px 24px; text-decoration:none; border-radius:8px; font-weight:bold; display:inline-block;'>
                        RESET ACCESS KEY
                    </a>
                </p>
                <p style='color:#94a3b8; font-size:12px;'>If you did not initiate this request, you can safely ignore this transmission.</p>
            </div>";

        return SendEmailAsync(toEmail, "Security Alert: Reset Your Password", html);
    }
}
