using AutoBolt.Application.DTOs;

namespace AutoBolt.Application.Interfaces;

public interface IInvoiceService
{
    Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync();
    Task<InvoiceDto?> GetInvoiceByIdAsync(int id);
    Task<InvoiceDto> CreateInvoiceAsync(InvoiceCreateDto dto);
    Task<bool> CancelInvoiceAsync(int id);
    Task<bool> EmailInvoiceAsync(int id, string? recipientEmail = null);
}
