namespace AutoBolt.Domain.Entities;

public class InvoiceItem : BaseEntity
{
    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
    
    public int PartId { get; set; }
    public Part? Part { get; set; }
    
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal => Quantity * UnitPrice;
}
