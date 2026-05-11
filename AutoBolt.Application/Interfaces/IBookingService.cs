using AutoBolt.Application.DTOs;
using AutoBolt.Domain.Enums;

namespace AutoBolt.Application.Interfaces;

public interface IBookingService
{
    Task<IEnumerable<BookingDto>> GetAllBookingsAsync();
    Task<IEnumerable<BookingDto>> GetUpcomingBookingsAsync();
    Task<BookingDto?> GetBookingByIdAsync(int id);
    Task<IEnumerable<BookingDto>> GetBookingsByCustomerIdAsync(int customerId);
    Task<BookingDto> CreateBookingAsync(BookingCreateDto dto);
    Task<bool> UpdateBookingStatusAsync(int id, BookingStatus status);
    Task<bool> DeleteBookingAsync(int id);
}
