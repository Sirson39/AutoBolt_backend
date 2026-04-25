namespace AutoBolt.Application.DTOs;

public class VehicleDto
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = null!;
    public string Make { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public double Mileage { get; set; }
    public string OwnerName { get; set; } = null!;
}

public class VehicleCreateUpdateDto
{
    public string LicensePlate { get; set; } = null!;
    public string Make { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public double Mileage { get; set; }
    public int CustomerId { get; set; }
}
