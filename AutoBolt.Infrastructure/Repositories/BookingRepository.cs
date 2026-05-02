using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using AutoBolt.Domain.Enums;
using AutoBolt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Infrastructure.Repositories;

public class BookingRepository(AutoBoltDbContext context) : GenericRepository<Booking>(context), IBookingRepository
{
    public async Task<IEnumerable<Booking>> GetUpcomingBookingsAsync()
    {
        return await _dbSet
            .Where(b => b.ServiceDate >= DateTime.UtcNow && b.Status != BookingStatus.Cancelled)
            .Include(b => b.Customer)
            .Include(b => b.Vehicle)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetBookingsByCustomerIdAsync(int customerId)
    {
        return await _dbSet
            .Where(b => b.CustomerId == customerId)
            .Include(b => b.Vehicle)
            .OrderByDescending(b => b.ServiceDate)
            .ToListAsync();
    }
}
