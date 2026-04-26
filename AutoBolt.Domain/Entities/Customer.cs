namespace AutoBolt.Domain.Entities;

public class Customer : BaseEntity
{
    public required string FullName { get; set; }
    public string? Email { get; set; }
    public required string Phone { get; set; }
    public string? Address { get; set; }
    public decimal CreditBalance { get; set; }
    
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
