using System.ComponentModel.DataAnnotations;

namespace AutoBolt.Application.DTOs;

public class VehicleDto
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = null!;
    public string Make { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public double Mileage { get; set; }
    public int PlateType { get; set; }
    public string OwnerName { get; set; } = null!;
}

public class VehicleCreateUpdateDto
{
    [Required(ErrorMessage = "License Plate is required.")]
    public string LicensePlate { get; set; } = null!;

    [Required(ErrorMessage = "Make is required.")]
    public string Make { get; set; } = null!;

    [Required(ErrorMessage = "Model is required.")]
    public string Model { get; set; } = null!;

    [Range(1900, 2100, ErrorMessage = "Please enter a valid year.")]
    public int Year { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Mileage cannot be negative.")]
    public double Mileage { get; set; }

    [Required(ErrorMessage = "Plate Type is required.")]
    public int PlateType { get; set; }

    [Required(ErrorMessage = "Customer Owner ID is required.")]
    public int CustomerId { get; set; }
}
