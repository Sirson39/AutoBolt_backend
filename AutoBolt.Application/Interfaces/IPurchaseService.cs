using System.Collections.Generic;
using System.Threading.Tasks;
using AutoBolt.Application.DTOs;

namespace AutoBolt.Application.Interfaces
{
    public interface IPurchaseService
    {
        Task<List<PurchaseInvoiceDto>> GetAllPurchaseInvoicesAsync();
        Task<PurchaseInvoiceDto> GetPurchaseInvoiceByIdAsync(int id);
        Task<PurchaseInvoiceDto> CreatePurchaseInvoiceAsync(CreatePurchaseInvoiceDto dto);
        Task<bool> DeletePurchaseInvoiceAsync(int id);
    }
}
