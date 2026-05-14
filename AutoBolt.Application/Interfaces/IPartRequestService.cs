using AutoBolt.Application.DTOs;
using AutoBolt.Domain.Enums;

namespace AutoBolt.Application.Interfaces;

public interface IPartRequestService
{
    Task<IEnumerable<PartRequestDto>> GetAllAsync();
    Task<IEnumerable<PartRequestDto>> GetByCustomerIdAsync(int customerId);
    Task<PartRequestDto> CreateAsync(PartRequestCreateDto dto);
    Task<bool> UpdateStatusAsync(int id, PartRequestStatus status);
    Task<bool> DeleteAsync(int id);
}
