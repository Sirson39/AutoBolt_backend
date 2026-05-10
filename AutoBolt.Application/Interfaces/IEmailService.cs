using AutoBolt.Application.DTOs;

namespace AutoBolt.Application.Interfaces;

public interface IEmailService
{
    Task SendStaffCredentialsAsync(string toEmail, string fullName, string password);
    Task SendPasswordResetEmailAsync(string toEmail, string fullName, string resetToken);
    Task SendResendCredentialsAsync(string toEmail, string fullName, string newPassword);
    Task SendInvoiceEmailAsync(string recipientEmail, InvoiceDto invoice);
}
