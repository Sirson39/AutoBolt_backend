using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using AutoBolt.Domain.Enums;
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
        return await _dbSet
            .Where(customer => customer.CreditBalance > 0)
            .Where(customer => _context.Invoices.Any(invoice =>
                invoice.CustomerId == customer.Id &&
                invoice.Status != InvoiceStatus.Paid &&
                invoice.Status != InvoiceStatus.Cancelled &&
                invoice.InvoiceDate <= DateTime.UtcNow.AddMonths(-1)))
            .ToListAsync();
    }
}
