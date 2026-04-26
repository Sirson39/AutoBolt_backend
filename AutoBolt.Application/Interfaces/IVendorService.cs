using AutoBolt.Application.DTOs;

namespace AutoBolt.Application.Interfaces;

public interface IVendorService
{
    Task<IEnumerable<VendorDto>> GetAllVendorsAsync();
    Task<VendorDto?> GetVendorByIdAsync(int id);
    Task<VendorDto> CreateVendorAsync(VendorCreateUpdateDto dto);
    Task UpdateVendorAsync(int id, VendorCreateUpdateDto dto);
    Task DeleteVendorAsync(int id);
}
