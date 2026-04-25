using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;

namespace AutoBolt.Application.Services;

public class CustomerService(ICustomerRepository customerRepository) : ICustomerService
{
    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await customerRepository.GetAllAsync();
        return customers.Select(MapToDto);
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        var customer = await customerRepository.GetByIdAsync(id);
        return customer != null ? MapToDto(customer) : null;
    }

    public async Task<CustomerDto> CreateCustomerAsync(CustomerCreateUpdateDto dto)
    {
        var customer = new Customer
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            CreditBalance = 0
        };

        await customerRepository.AddAsync(customer);
        await customerRepository.SaveChangesAsync();

        return MapToDto(customer);
    }

    public async Task UpdateCustomerAsync(int id, CustomerCreateUpdateDto dto)
    {
        var customer = await customerRepository.GetByIdAsync(id);
        if (customer == null) return;

        customer.FullName = dto.FullName;
        customer.Email = dto.Email;
        customer.Phone = dto.Phone;
        customer.Address = dto.Address;
        customer.UpdatedAt = DateTime.UtcNow;

        customerRepository.Update(customer);
        await customerRepository.SaveChangesAsync();
    }

    public async Task DeleteCustomerAsync(int id)
    {
        var customer = await customerRepository.GetByIdAsync(id);
        if (customer == null) return;

        customerRepository.Delete(customer);
        await customerRepository.SaveChangesAsync();
    }

    private static CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            CreditBalance = customer.CreditBalance
        };
    }
}
