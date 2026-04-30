using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using AutoBolt.Domain.Enums;

namespace AutoBolt.Application.Services;

public class PartService(IPartRepository partRepository) : IPartService
{
    public async Task<IEnumerable<PartDto>> GetAllPartsAsync()
    {
        var parts = await partRepository.GetAllAsync();
        return parts.Select(MapToDto);
    }

    public async Task<PartDto?> GetPartByIdAsync(int id)
    {
        var part = await partRepository.GetByIdAsync(id);
        return part != null ? MapToDto(part) : null;
    }

    public async Task<IEnumerable<PartDto>> SearchPartsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllPartsAsync();
        }

        var term = query.Trim();
        var parts = await partRepository.GetAllAsync();

        return parts
            .Where(part =>
                Contains(part.Name, term) ||
                Contains(part.Description, term) ||
                Contains(part.Category.ToString(), term) ||
                part.Id.ToString().Contains(term, StringComparison.OrdinalIgnoreCase))
            .Select(MapToDto);
    }

    public async Task<PartDto> CreatePartAsync(PartCreateUpdateDto dto)
    {
        var part = new Part
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            Category = (PartCategory)dto.CategoryId,
            ImageUrl = dto.ImageUrl
        };

        await partRepository.AddAsync(part);
        await partRepository.SaveChangesAsync();

        return MapToDto(part);
    }

    public async Task UpdatePartAsync(int id, PartCreateUpdateDto dto)
    {
        var part = await partRepository.GetByIdAsync(id);
        if (part == null) return;

        part.Name = dto.Name;
        part.Description = dto.Description;
        part.Price = dto.Price;
        part.StockQuantity = dto.StockQuantity;
        part.Category = (PartCategory)dto.CategoryId;
        part.ImageUrl = dto.ImageUrl ?? part.ImageUrl; // Keep existing if not updated
        part.UpdatedAt = DateTime.UtcNow;

        partRepository.Update(part);
        await partRepository.SaveChangesAsync();
    }

    public async Task DeletePartAsync(int id)
    {
        var part = await partRepository.GetByIdAsync(id);
        if (part == null) return;

        partRepository.Delete(part);
        await partRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<PartDto>> GetLowStockPartsAsync()
    {
        var parts = await partRepository.GetLowStockPartsAsync(10);
        return parts.Select(MapToDto);
    }

    private static PartDto MapToDto(Part part)
    {
        return new PartDto
        {
            Id = part.Id,
            Name = part.Name,
            Description = part.Description,
            Price = part.Price,
            StockQuantity = part.StockQuantity,
            Category = part.Category.ToString(),
            IsLowStock = part.IsLowStock,
            ImageUrl = part.ImageUrl
        };
    }

    private static bool Contains(string? source, string term)
    {
        return !string.IsNullOrWhiteSpace(source) &&
               source.Contains(term, StringComparison.OrdinalIgnoreCase);
    }
}
