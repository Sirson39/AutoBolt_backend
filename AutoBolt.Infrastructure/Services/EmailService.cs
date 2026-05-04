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

        await SendCoreAsync(message);
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetToken)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress(fullName, toEmail));
        message.Subject = "AutoBolt — Password Reset Request";

        message.Body = new TextPart("html")
        {
            Text = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2563eb;'>Password Reset Request</h2>
                    <p>Hi {fullName},</p>
                    <p>We received a request to reset your AutoBolt account password.
                       Use the token below with the reset-password form. This token expires shortly.</p>
                    <div style='background: #f3f4f6; padding: 16px; border-radius: 8px; margin: 16px 0; word-break: break-all;'>
                        <p style='margin: 0;'><strong>Reset Token:</strong></p>
                        <p style='font-family: monospace; color: #1d4ed8; margin: 8px 0 0 0;'>{resetToken}</p>
                    </div>
                    <p style='color: #ef4444;'>If you did not request a password reset, you can safely ignore this email.</p>
                    <hr/>
                    <p style='color: #6b7280; font-size: 12px;'>AutoBolt Vehicle Parts &amp; Service Management</p>
                </div>"
        };

        await SendCoreAsync(message);
    }

    public async Task SendResendCredentialsAsync(string toEmail, string fullName, string newPassword)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(new MailboxAddress(fullName, toEmail));
        message.Subject = "AutoBolt — Your New Login Credentials";

        message.Body = new TextPart("html")
        {
            Text = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2563eb;'>New Credentials — AutoBolt</h2>
                    <p>Hi {fullName},</p>
                    <p>An administrator has reset your password. Here are your updated login credentials:</p>
                    <div style='background: #f3f4f6; padding: 16px; border-radius: 8px; margin: 16px 0;'>
                        <p><strong>Email:</strong> {toEmail}</p>
                        <p><strong>New Password:</strong> {newPassword}</p>
                    </div>
                    <p style='color: #ef4444;'>Please log in and change your password immediately.</p>
                    <p>Login at: <a href='http://localhost:5173'>AutoBolt Portal</a></p>
                    <hr/>
                    <p style='color: #6b7280; font-size: 12px;'>AutoBolt Vehicle Parts &amp; Service Management</p>
                </div>"
        };

        await SendCoreAsync(message);
    }

    private async Task SendCoreAsync(MimeMessage message)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
