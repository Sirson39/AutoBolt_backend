using System.ComponentModel.DataAnnotations;

namespace AutoBolt.Application.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string Phone { get; set; } = null!;
    public string? Address { get; set; }
    public decimal CreditBalance { get; set; }
}

public class CustomerCreateUpdateDto
{
    [Required(ErrorMessage = "Full Name is required.")]
    public string FullName { get; set; } = null!;

    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string Phone { get; set; } = null!;

    public string? Address { get; set; }
}

public class CustomerHistoryDto
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string Phone { get; set; } = null!;
    public string? Address { get; set; }
    public decimal CreditBalance { get; set; }
    public List<CustomerVehicleHistoryDto> Vehicles { get; set; } = new();
    public List<CustomerInvoiceHistoryDto> Invoices { get; set; } = new();
}

public class CustomerVehicleHistoryDto
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = null!;
    public string Make { get; set; } = null!;
    public string Model { get; set; } = null!;
    public int Year { get; set; }
    public double Mileage { get; set; }
}

public class CustomerInvoiceHistoryDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public DateTime InvoiceDate { get; set; }
    public string VehiclePlate { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = null!;
}
