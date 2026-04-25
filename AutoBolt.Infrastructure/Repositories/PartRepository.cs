using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using AutoBolt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Infrastructure.Repositories;

public class PartRepository(AutoBoltDbContext context) : GenericRepository<Part>(context), IPartRepository
{
    public async Task<IEnumerable<Part>> GetLowStockPartsAsync(int threshold)
    {
        return await _dbSet
            .Where(p => p.StockQuantity < threshold)
            .ToListAsync();
    }
}
