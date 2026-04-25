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
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string Phone { get; set; } = null!;
    public string? Address { get; set; }
}
