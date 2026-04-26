using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;

namespace AutoBolt.Application.Services;

public class VendorService(IVendorRepository vendorRepository) : IVendorService
{
    public async Task<IEnumerable<VendorDto>> GetAllVendorsAsync()
    {
        var vendors = await vendorRepository.GetAllAsync();
        return vendors.Select(MapToDto);
    }

    public async Task<VendorDto?> GetVendorByIdAsync(int id)
    {
        var vendor = await vendorRepository.GetByIdAsync(id);
        return vendor != null ? MapToDto(vendor) : null;
    }

    public async Task<VendorDto> CreateVendorAsync(VendorCreateUpdateDto dto)
    {
        var vendor = new Vendor
        {
            Name = dto.Name,
            ContactPerson = dto.ContactPerson,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            LogoUrl = dto.LogoUrl
        };

        await vendorRepository.AddAsync(vendor);
        await vendorRepository.SaveChangesAsync();

        return MapToDto(vendor);
    }

    public async Task UpdateVendorAsync(int id, VendorCreateUpdateDto dto)
    {
        var vendor = await vendorRepository.GetByIdAsync(id);
        if (vendor == null) return;

        vendor.Name = dto.Name;
        vendor.ContactPerson = dto.ContactPerson;
        vendor.Email = dto.Email;
        vendor.Phone = dto.Phone;
        vendor.Address = dto.Address;
        
        // Only update logo if a new one was provided, otherwise keep existing
        if (dto.LogoUrl != null)
        {
            vendor.LogoUrl = dto.LogoUrl;
        }
        
        vendor.UpdatedAt = DateTime.UtcNow;

        vendorRepository.Update(vendor);
        await vendorRepository.SaveChangesAsync();
    }

    public async Task DeleteVendorAsync(int id)
    {
        var vendor = await vendorRepository.GetByIdAsync(id);
        if (vendor == null) return;

        vendorRepository.Delete(vendor);
        await vendorRepository.SaveChangesAsync();
    }

    private static VendorDto MapToDto(Vendor vendor)
    {
        return new VendorDto
        {
            Id = vendor.Id,
            Name = vendor.Name,
            ContactPerson = vendor.ContactPerson,
            Email = vendor.Email,
            Phone = vendor.Phone,
            Address = vendor.Address,
            LogoUrl = vendor.LogoUrl
        };
    }
}
