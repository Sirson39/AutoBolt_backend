namespace AutoBolt.Domain.Entities;

public class Vehicle : BaseEntity
{
    public required string LicensePlate { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public string? VIN { get; set; } // Vehicle Identification Number
    public double Mileage { get; set; } // Current mileage for AI prediction
    
    public int CustomerId { get; set; }
    public Customer? Owner { get; set; }
}
