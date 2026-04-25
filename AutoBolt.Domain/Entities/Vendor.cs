namespace AutoBolt.Domain.Entities;

public class Vendor : BaseEntity
{
    public required string Name { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    
    // Navigation property for parts supplied by this vendor (optional)
    public ICollection<Part> SuppliedParts { get; set; } = new List<Part>();
}
