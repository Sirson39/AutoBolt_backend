using AutoBolt.Application.DTOs;

namespace AutoBolt.Application.Interfaces;

public interface IServiceReviewService
{
    Task<IEnumerable<ServiceReviewDto>> GetAllAsync();
    Task<IEnumerable<ServiceReviewDto>> GetPublicAsync();
    Task<IEnumerable<ServiceReviewDto>> GetByCustomerIdAsync(int customerId);
    Task<ServiceReviewDto> CreateAsync(ServiceReviewCreateDto dto);
    Task<bool> DeleteAsync(int id);
}
