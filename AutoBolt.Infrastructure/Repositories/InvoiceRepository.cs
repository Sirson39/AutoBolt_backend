using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using AutoBolt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Infrastructure.Repositories;

public class InvoiceRepository(AutoBoltDbContext context) : GenericRepository<Invoice>(context), IInvoiceRepository
{
    public async Task<IEnumerable<Invoice>> GetInvoicesByDateRangeAsync(DateTime start, DateTime end)
    {
        return await _dbSet
            .Where(i => i.InvoiceDate >= start && i.InvoiceDate <= end)
            .Include(i => i.Items)
            .ThenInclude(ii => ii.Part)
            .ToListAsync();
    }
}
