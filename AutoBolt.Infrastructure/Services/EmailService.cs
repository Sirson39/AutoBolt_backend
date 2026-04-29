using AutoBolt.Application.Interfaces;
using AutoBolt.Infrastructure.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AutoBolt.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendStaffCredentialsAsync(string toEmail, string fullName, string password)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress(fullName, toEmail));
        message.Subject = "Your AutoBolt Staff Account Credentials";

        message.Body = new TextPart("html")
        {
            Text = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2563eb;'>Welcome to AutoBolt, {fullName}!</h2>
                    <p>Your staff account has been created. Here are your login credentials:</p>
                    <div style='background: #f3f4f6; padding: 16px; border-radius: 8px; margin: 16px 0;'>
                        <p><strong>Email:</strong> {toEmail}</p>
                        <p><strong>Password:</strong> {password}</p>
                    </div>
                    <p style='color: #ef4444;'>Please change your password after your first login.</p>
                    <p>Login at: <a href='http://localhost:5173'>AutoBolt Portal</a></p>
                    <hr/>
                    <p style='color: #6b7280; font-size: 12px;'>AutoBolt Vehicle Parts &amp; Service Management</p>
                </div>"
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
