using AutoBolt.Domain.Enums;

namespace AutoBolt.Domain.Entities;

public class Part : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public PartCategory Category { get; set; }
    public string? ImageUrl { get; set; }
    
    // Low stock threshold logic will be in Application layer, 
    // but we can mark it here if needed.
    public bool IsLowStock => StockQuantity < 10;
}
