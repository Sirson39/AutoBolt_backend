using AutoBolt.Application.DTOs;

namespace AutoBolt.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string recipientEmail, string subject, string htmlBody);
    Task SendInvoiceEmailAsync(string recipientEmail, InvoiceDto invoice);
}
