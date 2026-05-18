using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBolt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class BookingsController(IBookingService bookingService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetAll()
    {
        var bookings = await bookingService.GetAllBookingsAsync();
        return Ok(bookings);
    }

    [HttpGet("upcoming")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetUpcoming()
    {
        var bookings = await bookingService.GetUpcomingBookingsAsync();
        return Ok(bookings);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookingDto>> GetById(int id)
    {
        var booking = await bookingService.GetBookingByIdAsync(id);
        if (booking == null) return NotFound();
        return Ok(booking);
    }

    [HttpGet("customer/{customerId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<BookingDto>>> GetByCustomer(int customerId)
    {
        var bookings = await bookingService.GetBookingsByCustomerIdAsync(customerId);
        return Ok(bookings);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<BookingDto>> Create(BookingCreateDto dto)
    {
        try
        {
            var booking = await bookingService.CreateBookingAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateStatus(int id, BookingUpdateStatusDto dto)
    {
        var result = await bookingService.UpdateBookingStatusAsync(id, dto.Status);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await bookingService.DeleteBookingAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
