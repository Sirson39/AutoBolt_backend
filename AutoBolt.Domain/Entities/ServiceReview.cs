namespace AutoBolt.Domain.Entities;

public class ServiceReview : BaseEntity
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsPublic { get; set; } = true;

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }
}
