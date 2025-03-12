using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

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
        var smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER") 
                         ?? _configuration["EmailSettings:SmtpServer"];

        var smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") 
                         ?? _configuration["EmailSettings:SmtpPort"]!);

        var smtpUser = Environment.GetEnvironmentVariable("SMTP_USER");
        var smtpPass = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

        var fromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL") 
                         ?? _configuration["EmailSettings:FromEmail"];

        var useStartTls = bool.Parse(Environment.GetEnvironmentVariable("USE_STARTTLS") 
                         ?? _configuration["EmailSettings:UseStartTls"]!);

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(fromEmail));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlMessage };

        using var smtpClient = new SmtpClient();

        if (useStartTls)
        {
            await smtpClient.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            if (!string.IsNullOrEmpty(smtpUser) && !string.IsNullOrEmpty(smtpPass))
                await smtpClient.AuthenticateAsync(smtpUser, smtpPass);
        }
        else
        {
            await smtpClient.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.None);
        }

        await smtpClient.SendAsync(message);
        await smtpClient.DisconnectAsync(true);
    }
}
