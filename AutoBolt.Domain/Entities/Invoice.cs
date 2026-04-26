using AutoBolt.Domain.Enums;

namespace AutoBolt.Domain.Entities;

public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = null!; // e.g. INV-2026-001
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    
    public int? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public InvoiceStatus Status { get; set; }
    
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
}
