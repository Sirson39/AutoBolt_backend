using AutoBolt.Application.DTOs;

namespace AutoBolt.Application.Interfaces;

public interface IEmailService
{
    Task SendInvoiceEmailAsync(string recipientEmail, InvoiceDto invoice);
}
