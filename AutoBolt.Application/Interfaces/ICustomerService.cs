using AutoBolt.Application.DTOs;

namespace AutoBolt.Application.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string query);
    Task<CustomerDto> CreateCustomerAsync(CustomerCreateUpdateDto dto);
    Task UpdateCustomerAsync(int id, CustomerCreateUpdateDto dto);
    Task DeleteCustomerAsync(int id);
}
