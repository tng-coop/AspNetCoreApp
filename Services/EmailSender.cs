using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace AspNetCoreApp.Services;

public class EmailSender(IConfiguration configuration) : IEmailSender
{
    private readonly IConfiguration _configuration = configuration;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var smtpServer = _configuration["EmailSettings:SmtpServer"];
        var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        var smtpUser = _configuration["EmailSettings:SmtpUser"];
        var smtpPass = _configuration["EmailSettings:SmtpPassword"];
        var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "no-reply@example.com";
        bool useStartTls = bool.Parse(_configuration["EmailSettings:UseStartTls"] ?? "true");

        if (string.IsNullOrEmpty(smtpServer))
            throw new InvalidOperationException("SMTP settings missing in configuration.");

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(fromEmail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlMessage };
        message.Body = builder.ToMessageBody();

        using var smtpClient = new MailKit.Net.Smtp.SmtpClient();

        var options = useStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None;

        await smtpClient.ConnectAsync(smtpServer, smtpPort, options);

        if (!string.IsNullOrEmpty(smtpUser))
        {
            await smtpClient.AuthenticateAsync(smtpUser, smtpPass);
        }

        await smtpClient.SendAsync(message);
        await smtpClient.DisconnectAsync(true);
    }
}
