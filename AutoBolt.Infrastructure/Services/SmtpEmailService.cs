using System.Net;
using System.Net.Mail;
using System.Text;
using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AutoBolt.Infrastructure.Services;

public class SmtpEmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendInvoiceEmailAsync(string recipientEmail, InvoiceDto invoice)
    {
        var host = configuration["Smtp:Host"];
        var port = int.TryParse(configuration["Smtp:Port"], out var parsedPort) ? parsedPort : 587;
        var username = configuration["Smtp:Username"];
        var password = configuration["Smtp:Password"];
        var fromEmail = configuration["Smtp:FromEmail"] ?? username;
        var fromName = configuration["Smtp:FromName"] ?? "AutoBolt";
        var enableSsl = !bool.TryParse(configuration["Smtp:EnableSsl"], out var parsedSsl) || parsedSsl;

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(fromEmail))
        {
            throw new InvalidOperationException("SMTP settings are not configured.");
        }

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = $"Your AutoBolt Invoice {invoice.InvoiceNumber}",
            Body = BuildInvoiceHtml(invoice),
            IsBodyHtml = true
        };

        message.To.Add(recipientEmail);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            Credentials = new NetworkCredential(username, password)
        };

        await client.SendMailAsync(message);
    }

    private static string BuildInvoiceHtml(InvoiceDto invoice)
    {
        var items = new StringBuilder();
        foreach (var item in invoice.Items)
        {
            items.AppendLine($@"
                <tr>
                    <td>{WebUtility.HtmlEncode(item.PartName)}</td>
                    <td style=""text-align:right;"">{item.Quantity}</td>
                    <td style=""text-align:right;"">{item.UnitPrice:0.00}</td>
                    <td style=""text-align:right;"">{item.SubTotal:0.00}</td>
                </tr>");
        }

        return $@"
            <html>
            <body style=""font-family: Arial, sans-serif; color: #1f2937;"">
                <h2>AutoBolt Invoice {WebUtility.HtmlEncode(invoice.InvoiceNumber)}</h2>
                <p><strong>Date:</strong> {invoice.InvoiceDate:yyyy-MM-dd HH:mm}</p>
                <p><strong>Customer:</strong> {WebUtility.HtmlEncode(invoice.CustomerName)}</p>
                <p><strong>Vehicle:</strong> {WebUtility.HtmlEncode(invoice.VehiclePlate ?? "N/A")}</p>
                <table style=""width:100%; border-collapse: collapse; margin-top: 16px;"">
                    <thead>
                        <tr>
                            <th style=""text-align:left; border-bottom:1px solid #d1d5db; padding:8px;"">Part</th>
                            <th style=""text-align:right; border-bottom:1px solid #d1d5db; padding:8px;"">Qty</th>
                            <th style=""text-align:right; border-bottom:1px solid #d1d5db; padding:8px;"">Unit Price</th>
                            <th style=""text-align:right; border-bottom:1px solid #d1d5db; padding:8px;"">Subtotal</th>
                        </tr>
                    </thead>
                    <tbody>
                        {items}
                    </tbody>
                </table>
                <div style=""margin-top:16px;"">
                    <p><strong>SubTotal:</strong> {invoice.SubTotal:0.00}</p>
                    <p><strong>Discount:</strong> {invoice.DiscountAmount:0.00}</p>
                    <p><strong>Tax:</strong> {invoice.TaxAmount:0.00}</p>
                    <p><strong>Total:</strong> {invoice.TotalAmount:0.00}</p>
                </div>
            </body>
            </html>";
    }
}
