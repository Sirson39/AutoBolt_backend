using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using AutoBolt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Infrastructure.Services;

public class ServiceReviewService(AutoBoltDbContext context) : IServiceReviewService
{
    public async Task<IEnumerable<ServiceReviewDto>> GetAllAsync()
    {
        return await context.ServiceReviews
            .Include(r => r.Customer)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => Map(r))
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceReviewDto>> GetPublicAsync()
    {
        return await context.ServiceReviews
            .Include(r => r.Customer)
            .Where(r => r.IsPublic)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => Map(r))
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceReviewDto>> GetByCustomerIdAsync(int customerId)
    {
        return await context.ServiceReviews
            .Include(r => r.Customer)
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => Map(r))
            .ToListAsync();
    }

    public async Task<ServiceReviewDto> CreateAsync(ServiceReviewCreateDto dto)
    {
        var review = new ServiceReview
        {
            Rating = dto.Rating,
            Comment = dto.Comment,
            CustomerId = dto.CustomerId,
            InvoiceId = dto.InvoiceId,
            IsPublic = true
        };
        context.ServiceReviews.Add(review);
        await context.SaveChangesAsync();

        await context.Entry(review).Reference(r => r.Customer).LoadAsync();
        return Map(review);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var review = await context.ServiceReviews.FindAsync(id);
        if (review == null) return false;
        context.ServiceReviews.Remove(review);
        await context.SaveChangesAsync();
        return true;
    }

    private static ServiceReviewDto Map(ServiceReview r) => new()
    {
        Id = r.Id,
        Rating = r.Rating,
        Comment = r.Comment,
        IsPublic = r.IsPublic,
        CustomerId = r.CustomerId,
        CustomerName = r.Customer?.FullName ?? string.Empty,
        InvoiceId = r.InvoiceId,
        CreatedAt = r.CreatedAt
    };
}
