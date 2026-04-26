using AutoBolt.Domain.Entities;
using AutoBolt.Application.Interfaces;
using AutoBolt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Infrastructure.Repositories;

public class VehicleRepository(AutoBoltDbContext context) : GenericRepository<Vehicle>(context), IVehicleRepository
{
    public override async Task<IEnumerable<Vehicle>> GetAllAsync()
    {
        return await _dbSet
            .Include(v => v.Owner)
            .ToListAsync();
    }

    public override async Task<Vehicle?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(v => v.Owner)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task<IEnumerable<Vehicle>> GetVehiclesByCustomerIdAsync(int customerId)
    {
        return await _dbSet
            .Where(v => v.CustomerId == customerId)
            .ToListAsync();
    }
}
