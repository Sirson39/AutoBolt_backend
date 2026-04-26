using System.ComponentModel.DataAnnotations;

namespace AutoBolt.Application.DTOs;

public class VendorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? LogoUrl { get; set; }
}

public class VendorCreateUpdateDto
{
    [Required]
    public string Name { get; set; } = null!;
    
    public string? ContactPerson { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    
    [Phone]
    public string? Phone { get; set; }
    
    public string? Address { get; set; }
    public string? LogoUrl { get; set; }
}
