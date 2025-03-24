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
        var smtpServer = _configuration["SmtpSettings:Server"];
        var smtpPortString = _configuration["SmtpSettings:Port"];
        if (!int.TryParse(smtpPortString, out var smtpPort))
        {
            throw new InvalidOperationException("Invalid SMTP port configuration.");
        }
        var smtpUser = _configuration["SmtpSettings:User"];
        var smtpPass = _configuration["SmtpSettings:Password"];
        var fromEmail = _configuration["SmtpSettings:FromEmail"];
        var UseStartTls = _configuration["SmtpSettings:UseStartTls"];

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(fromEmail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlMessage };

        using var smtpClient = new SmtpClient();
        if (UseStartTls == "true")
        {
            await smtpClient.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(smtpUser, smtpPass);
        }
        else
        { await smtpClient.ConnectAsync(smtpServer, smtpPort); }
        await smtpClient.SendAsync(message);
        await smtpClient.DisconnectAsync(true);
    }
}
