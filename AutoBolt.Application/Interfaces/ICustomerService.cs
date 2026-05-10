using AutoBolt.Application.DTOs;

namespace AutoBolt.Application.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string query);
    Task<IEnumerable<CustomerDto>> GetOverdueCreditCustomersAsync();
    Task<CustomerHistoryDto?> GetCustomerHistoryAsync(int id);
    Task<CustomerSummaryDto?> GetCustomerSummaryAsync(int id);
    Task<CustomerDto> CreateCustomerAsync(CustomerCreateUpdateDto dto);
    Task<CustomerRegistrationResultDto> RegisterCustomerWithVehicleAsync(CustomerRegistrationDto dto);
    Task<CustomerCreditPaymentResultDto> ApplyCreditPaymentAsync(int id, CustomerCreditPaymentDto dto);
    Task UpdateCustomerAsync(int id, CustomerCreateUpdateDto dto);
    Task DeleteCustomerAsync(int id);
}
