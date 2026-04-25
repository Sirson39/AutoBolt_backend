using AutoBolt.Domain.Entities;

namespace AutoBolt.Application.Interfaces;

public interface IInvoiceRepository : IGenericRepository<Invoice>
{
    Task<IEnumerable<Invoice>> GetInvoicesByDateRangeAsync(DateTime start, DateTime end);
}
