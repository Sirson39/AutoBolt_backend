using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using AutoBolt.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using AutoBolt.Infrastructure.Data;

namespace AutoBolt.Infrastructure.Services;

public class BookingService(
    IBookingRepository bookingRepository,
    IGenericRepository<Customer> customerRepository,
    IGenericRepository<Vehicle> vehicleRepository) : IBookingService
{
    public async Task<IEnumerable<BookingDto>> GetAllBookingsAsync()
    {
        var bookings = await bookingRepository.GetAllAsync();
        var result = new List<BookingDto>();
        foreach (var b in bookings)
        {
            result.Add(await MapToDtoAsync(b));
        }
        return result;
    }

    public async Task<IEnumerable<BookingDto>> GetUpcomingBookingsAsync()
    {
        var bookings = await bookingRepository.GetUpcomingBookingsAsync();
        var result = new List<BookingDto>();
        foreach (var b in bookings)
        {
            result.Add(MapToDto(b));
        }
        return result;
    }

    public async Task<BookingDto?> GetBookingByIdAsync(int id)
    {
        var booking = await bookingRepository.GetByIdAsync(id);
        if (booking == null) return null;
        return await MapToDtoAsync(booking);
    }

    public async Task<IEnumerable<BookingDto>> GetBookingsByCustomerIdAsync(int customerId)
    {
        var bookings = await bookingRepository.GetBookingsByCustomerIdAsync(customerId);
        var result = new List<BookingDto>();
        foreach (var b in bookings)
        {
            result.Add(MapToDto(b));
        }
        return result;
    }

    public async Task<BookingDto> CreateBookingAsync(BookingCreateDto dto)
    {
        var customer = await customerRepository.GetByIdAsync(dto.CustomerId)
            ?? throw new InvalidOperationException($"Customer {dto.CustomerId} not found.");
        var vehicle = await vehicleRepository.GetByIdAsync(dto.VehicleId)
            ?? throw new InvalidOperationException($"Vehicle {dto.VehicleId} not found.");

        var booking = new Booking
        {
            ServiceDate = dto.ServiceDate,
            Description = dto.Description,
            Status = BookingStatus.Pending,
            CustomerId = dto.CustomerId,
            VehicleId = dto.VehicleId,
            Customer = customer,
            Vehicle = vehicle
        };

        await bookingRepository.AddAsync(booking);
        await bookingRepository.SaveChangesAsync();

        return MapToDto(booking);
    }

    public async Task<bool> UpdateBookingStatusAsync(int id, BookingStatus status)
    {
        var booking = await bookingRepository.GetByIdAsync(id);
        if (booking == null) return false;

        booking.Status = status;
        booking.UpdatedAt = DateTime.UtcNow;
        bookingRepository.Update(booking);
        await bookingRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteBookingAsync(int id)
    {
        var booking = await bookingRepository.GetByIdAsync(id);
        if (booking == null) return false;

        bookingRepository.Delete(booking);
        await bookingRepository.SaveChangesAsync();
        return true;
    }

    private static BookingDto MapToDto(Booking b) => new()
    {
        Id = b.Id,
        ServiceDate = b.ServiceDate,
        Description = b.Description,
        Status = b.Status.ToString(),
        CustomerId = b.CustomerId,
        CustomerName = b.Customer?.FullName ?? string.Empty,
        VehicleId = b.VehicleId,
        VehiclePlate = b.Vehicle?.LicensePlate ?? string.Empty,
        CreatedAt = b.CreatedAt
    };

    private async Task<BookingDto> MapToDtoAsync(Booking b)
    {
        var customerName = b.Customer?.FullName;
        var vehiclePlate = b.Vehicle?.LicensePlate;

        if (customerName == null)
        {
            var customer = await customerRepository.GetByIdAsync(b.CustomerId);
            customerName = customer?.FullName ?? string.Empty;
        }

        if (vehiclePlate == null)
        {
            var vehicle = await vehicleRepository.GetByIdAsync(b.VehicleId);
            vehiclePlate = vehicle?.LicensePlate ?? string.Empty;
        }

        return new BookingDto
        {
            Id = b.Id,
            ServiceDate = b.ServiceDate,
            Description = b.Description,
            Status = b.Status.ToString(),
            CustomerId = b.CustomerId,
            CustomerName = customerName,
            VehicleId = b.VehicleId,
            VehiclePlate = vehiclePlate,
            CreatedAt = b.CreatedAt
        };
    }
}
