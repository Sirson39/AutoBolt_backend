using AutoBolt.Domain.Entities;

namespace AutoBolt.Application.Interfaces;

public interface ICustomerRepository : IGenericRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(string email);
    Task<IEnumerable<Customer>> GetCustomersWithOverdueCreditsAsync();
}
