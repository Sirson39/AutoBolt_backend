using AutoBolt.Domain.Enums;

namespace AutoBolt.Domain.Entities;

public class Vehicle : BaseEntity
{
    public required string LicensePlate { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public string? VIN { get; set; }
    public double Mileage { get; set; }
    public PlateType PlateType { get; set; }
    
    public int CustomerId { get; set; }
    public Customer? Owner { get; set; }
}
