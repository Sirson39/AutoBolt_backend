using System.ComponentModel.DataAnnotations;

namespace AutoBolt.API.DTOs;

public class VendorCreateRequest
{
    [Required]
    public string Name { get; set; } = null!;
    
    public string? ContactPerson { get; set; }
    
    [EmailAddress]
    public string? Email { get; set; }
    
    [Phone]
    public string? Phone { get; set; }
    
    public string? Address { get; set; }
    
    public IFormFile? Logo { get; set; }
}
