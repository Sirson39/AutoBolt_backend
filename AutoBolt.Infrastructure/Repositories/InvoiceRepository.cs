using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using AutoBolt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Infrastructure.Repositories;

public class InvoiceRepository(AutoBoltDbContext context) : GenericRepository<Invoice>(context), IInvoiceRepository
{
    public async Task<IEnumerable<Invoice>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(i => i.Customer)
            .Include(i => i.Vehicle)
            .Include(i => i.Items)
                .ThenInclude(ii => ii.Part)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<Invoice?> GetByIdWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(i => i.Customer)
            .Include(i => i.Vehicle)
            .Include(i => i.Items)
                .ThenInclude(ii => ii.Part)
            .FirstOrDefaultAsync(i => i.Id == id);
    }
}
