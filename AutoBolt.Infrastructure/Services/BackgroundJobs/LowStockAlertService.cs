using AutoBolt.Application.Interfaces;
using AutoBolt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AutoBolt.Infrastructure.Services.BackgroundJobs;

public class LowStockAlertService : BackgroundService
{
    private readonly ILogger<LowStockAlertService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public LowStockAlertService(ILogger<LowStockAlertService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LowStockAlertService starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessLowStockAlertsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing LowStockAlertService.");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task ProcessLowStockAlertsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AutoBoltDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var lowStockParts = await context.Parts
            .Where(p => p.StockQuantity < 10)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync(stoppingToken);

        if (!lowStockParts.Any())
        {
            _logger.LogInformation("No parts are currently low on stock.");
            return;
        }

        var html = new StringBuilder();
        html.AppendLine("<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>");
        html.AppendLine("<h2 style='color: #ea580c;'>Low Stock Alert</h2>");
        html.AppendLine($"<p>{lowStockParts.Count} part(s) are running low (fewer than 10 units). Please restock soon.</p>");
        html.AppendLine("<table style='border-collapse: collapse; width: 100%; margin-top: 12px;'>");
        html.AppendLine("<thead><tr style='background: #f3f4f6;'><th style='padding: 8px; border: 1px solid #d1d5db; text-align: left;'>Part</th><th style='padding: 8px; border: 1px solid #d1d5db;'>Category</th><th style='padding: 8px; border: 1px solid #d1d5db;'>Stock</th></tr></thead>");
        html.AppendLine("<tbody>");

        foreach (var part in lowStockParts)
        {
            var stockColor = part.StockQuantity == 0 ? "#dc2626" : "#ea580c";
            html.AppendLine($"<tr><td style='padding: 8px; border: 1px solid #d1d5db;'>{part.Name}</td><td style='padding: 8px; border: 1px solid #d1d5db; text-align: center;'>{part.Category}</td><td style='padding: 8px; border: 1px solid #d1d5db; text-align: center; color: {stockColor}; font-weight: bold;'>{part.StockQuantity}</td></tr>");
        }

        html.AppendLine("</tbody></table>");
        html.AppendLine("<hr/><p style='color: #6b7280; font-size: 12px;'>AutoBolt Vehicle Parts &amp; Service Management</p></div>");

        await emailService.SendEmailAsync(
            "admin@autobolt.com",
            $"AutoBolt — Low Stock Alert ({lowStockParts.Count} items)",
            html.ToString());

        _logger.LogInformation("LowStockAlertService alerted admin for {Count} low-stock parts.", lowStockParts.Count);
    }
}
