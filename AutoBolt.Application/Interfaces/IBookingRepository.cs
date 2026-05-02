using AutoBolt.Domain.Entities;

namespace AutoBolt.Application.Interfaces;

public interface IBookingRepository : IGenericRepository<Booking>
{
    Task<IEnumerable<Booking>> GetUpcomingBookingsAsync();
    Task<IEnumerable<Booking>> GetBookingsByCustomerIdAsync(int customerId);
}
