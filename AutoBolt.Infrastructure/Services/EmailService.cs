using AutoBolt.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutoBolt.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        // For development, we just log the email
        _logger.LogInformation("Sending email to {To}: {Subject}\n{Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
