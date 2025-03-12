using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MailKit.Net.Smtp;

namespace AspNetCoreApp.Services;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "localhost";
        var smtpPort = int.TryParse(_configuration["EmailSettings:SmtpPort"], out var port) ? port : 25;
        var smtpUser = _configuration["EmailSettings:SmtpUser"];
        var smtpPass = _configuration["EmailSettings:SmtpPassword"];
        var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "no-reply@example.com";
        var useStartTls = bool.TryParse(_configuration["EmailSettings:UseStartTls"], out var tls) && tls;

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(fromEmail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlMessage };

        using var smtpClient = new SmtpClient();

        if (useStartTls)
        {
            await smtpClient.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
        }
        else
        {
            await smtpClient.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.Auto);
        }

        var smtpUserProvided = !string.IsNullOrEmpty(smtpUser);
        var smtpPassProvided = !string.IsNullOrEmpty(smtpPass);

        if (smtpUserProvided && smtpPassProvided)
        {
            await smtpClient.AuthenticateAsync(smtpUser, smtpPass);
        }

        await smtpClient.SendAsync(message);
        await smtpClient.DisconnectAsync(true);
    }
}
