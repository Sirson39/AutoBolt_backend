using AutoBolt.Application.DTOs;

namespace AutoBolt.Application.Interfaces;

public interface IPartService
{
    Task<IEnumerable<PartDto>> GetAllPartsAsync();
    Task<PartDto?> GetPartByIdAsync(int id);
    Task<PartDto> CreatePartAsync(PartCreateUpdateDto dto);
    Task UpdatePartAsync(int id, PartCreateUpdateDto dto);
    Task DeletePartAsync(int id);
    Task<IEnumerable<PartDto>> GetLowStockPartsAsync();
}
