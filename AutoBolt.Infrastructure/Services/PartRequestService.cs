using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using AutoBolt.Domain.Enums;
using AutoBolt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Infrastructure.Services;

public class PartRequestService(AutoBoltDbContext context) : IPartRequestService
{
    public async Task<IEnumerable<PartRequestDto>> GetAllAsync()
    {
        return await context.PartRequests
            .Include(p => p.Customer)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => Map(p))
            .ToListAsync();
    }

    public async Task<IEnumerable<PartRequestDto>> GetByCustomerIdAsync(int customerId)
    {
        return await context.PartRequests
            .Include(p => p.Customer)
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => Map(p))
            .ToListAsync();
    }

    public async Task<PartRequestDto> CreateAsync(PartRequestCreateDto dto)
    {
        var request = new PartRequest
        {
            PartName = dto.PartName,
            Description = dto.Description,
            Quantity = dto.Quantity,
            CustomerId = dto.CustomerId,
            Status = PartRequestStatus.Pending
        };
        context.PartRequests.Add(request);
        await context.SaveChangesAsync();

        await context.Entry(request).Reference(r => r.Customer).LoadAsync();
        return Map(request);
    }

    public async Task<bool> UpdateStatusAsync(int id, PartRequestStatus status)
    {
        var request = await context.PartRequests.FindAsync(id);
        if (request == null) return false;
        request.Status = status;
        request.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var request = await context.PartRequests.FindAsync(id);
        if (request == null) return false;
        context.PartRequests.Remove(request);
        await context.SaveChangesAsync();
        return true;
    }

    private static PartRequestDto Map(PartRequest p) => new()
    {
        Id = p.Id,
        PartName = p.PartName,
        Description = p.Description,
        Quantity = p.Quantity,
        Status = p.Status.ToString(),
        CustomerId = p.CustomerId,
        CustomerName = p.Customer?.FullName ?? string.Empty,
        CreatedAt = p.CreatedAt
    };
}
