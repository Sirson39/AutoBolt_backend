using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;

namespace AutoBolt.Application.Services;

public class CustomerService(ICustomerRepository customerRepository, IVehicleRepository vehicleRepository) : ICustomerService
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

    public async Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllCustomersAsync();
        }

        var term = query.Trim();
        var customers = await customerRepository.GetAllAsync();
        var vehicles = await vehicleRepository.GetAllAsync();

        var matchedCustomerIds = vehicles
            .Where(vehicle => Contains(vehicle.LicensePlate, term))
            .Select(vehicle => vehicle.CustomerId)
            .ToHashSet();

        return customers
            .Where(customer =>
                Contains(customer.FullName, term) ||
                Contains(customer.Email, term) ||
                Contains(customer.Phone, term) ||
                Contains(customer.Address, term) ||
                matchedCustomerIds.Contains(customer.Id))
            .Select(MapToDto);
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

    private static bool Contains(string? source, string term)
    {
        return !string.IsNullOrWhiteSpace(source) &&
               source.Contains(term, StringComparison.OrdinalIgnoreCase);
    }
}
