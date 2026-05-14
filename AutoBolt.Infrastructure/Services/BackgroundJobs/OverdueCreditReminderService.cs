using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Enums;
using AutoBolt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AutoBolt.Infrastructure.Services.BackgroundJobs;

public class OverdueCreditReminderService : BackgroundService
{
    private readonly ILogger<OverdueCreditReminderService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public OverdueCreditReminderService(ILogger<OverdueCreditReminderService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OverdueCreditReminderService starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOverdueRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing OverdueCreditReminderService.");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task ProcessOverdueRemindersAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AutoBoltDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var cutoff = DateTime.UtcNow.AddDays(-30);
        var overdueInvoices = await context.Invoices
            .Include(i => i.Customer)
            .Where(i => i.Status == InvoiceStatus.Pending && i.InvoiceDate < cutoff)
            .ToListAsync(stoppingToken);

        if (!overdueInvoices.Any())
        {
            _logger.LogInformation("No overdue invoices found.");
            return;
        }

        int sentCount = 0;
        foreach (var invoice in overdueInvoices)
        {
            var email = invoice.Customer?.Email;
            if (string.IsNullOrWhiteSpace(email)) continue;

            var subject = $"Action Required: Overdue Invoice {invoice.InvoiceNumber}";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #dc2626;'>Payment Reminder</h2>
                    <p>Dear {invoice.Customer!.FullName},</p>
                    <p>Invoice <strong>{invoice.InvoiceNumber}</strong> dated <strong>{invoice.InvoiceDate:yyyy-MM-dd}</strong>
                       for <strong>Rs {invoice.TotalAmount:N2}</strong> is now overdue by more than 30 days.</p>
                    <p>Please arrange payment at your earliest convenience.
                       If you have already paid, please disregard this notice.</p>
                    <br/>
                    <p>Thank you,<br/>AutoBolt Management</p>
                    <hr/>
                    <p style='color: #6b7280; font-size: 12px;'>AutoBolt Vehicle Parts &amp; Service Management</p>
                </div>";

            try
            {
                await emailService.SendEmailAsync(email, subject, body);
                sentCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send reminder for invoice {InvoiceId}", invoice.Id);
            }
        }

        _logger.LogInformation("OverdueCreditReminderService sent {Count}/{Total} reminders.", sentCount, overdueInvoices.Count);
    }
}
