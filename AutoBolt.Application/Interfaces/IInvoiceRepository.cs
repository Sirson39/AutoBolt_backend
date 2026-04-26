using AutoBolt.Domain.Entities;

namespace AutoBolt.Application.Interfaces;

public interface IInvoiceRepository : IGenericRepository<Invoice>
{
    Task<IEnumerable<Invoice>> GetAllWithDetailsAsync();
    Task<Invoice?> GetByIdWithDetailsAsync(int id);
}
