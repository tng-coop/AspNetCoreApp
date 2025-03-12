using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

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
        var smtpServer = _configuration["EmailSettings:Server"];
        var smtpPortString = _configuration["EmailSettings:Port"];
        if (!int.TryParse(smtpPortString, out var smtpPort))
        {
            throw new InvalidOperationException("Invalid SMTP port configuration.");
        }
        var smtpUser = _configuration["EmailSettings:User"];
        var smtpPass = _configuration["EmailSettings:Password"];
        var fromEmail = _configuration["EmailSettings:FromEmail"];

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(fromEmail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlMessage };

        using var smtpClient = new SmtpClient();

        await smtpClient.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
        await smtpClient.AuthenticateAsync(smtpUser, smtpPass);
        await smtpClient.SendAsync(message);
        await smtpClient.DisconnectAsync(true);
    }
}
