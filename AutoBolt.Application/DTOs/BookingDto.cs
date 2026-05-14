using System.ComponentModel.DataAnnotations;
using AutoBolt.Domain.Enums;

namespace AutoBolt.Application.DTOs;

public class BookingDto
{
    public int Id { get; set; }
    public DateTime ServiceDate { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int VehicleId { get; set; }
    public string VehiclePlate { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class BookingCreateDto
{
    [Required] public DateTime ServiceDate { get; set; }
    public string? Description { get; set; }
    [Required] public int CustomerId { get; set; }
    [Required] public int VehicleId { get; set; }
}

public class BookingUpdateStatusDto
{
    [Required] public BookingStatus Status { get; set; }
}
