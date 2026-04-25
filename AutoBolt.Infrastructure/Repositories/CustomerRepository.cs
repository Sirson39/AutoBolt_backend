using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using AutoBolt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Infrastructure.Repositories;

public class CustomerRepository(AutoBoltDbContext context) : GenericRepository<Customer>(context), ICustomerRepository
{
    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<IEnumerable<Customer>> GetCustomersWithOverdueCreditsAsync()
    {
        // For 100/100: Filter customers where credit balance > 0 and 
        // they have an unpaid invoice older than 1 month.
        return await _dbSet
            .Where(c => c.CreditBalance > 0)
            .ToListAsync();
    }
}
