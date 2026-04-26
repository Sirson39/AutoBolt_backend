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
