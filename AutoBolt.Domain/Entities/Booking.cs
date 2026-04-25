using AutoBolt.Domain.Enums;

namespace AutoBolt.Domain.Entities;

public class Booking : BaseEntity
{
    public DateTime ServiceDate { get; set; }
    public string? Description { get; set; }
    public BookingStatus Status { get; set; }
    
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    
    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
}
