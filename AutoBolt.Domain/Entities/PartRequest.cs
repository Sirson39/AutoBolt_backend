using AutoBolt.Domain.Enums;

namespace AutoBolt.Domain.Entities;

public class PartRequest : BaseEntity
{
    public string PartName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quantity { get; set; } = 1;
    public PartRequestStatus Status { get; set; } = PartRequestStatus.Pending;

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
}
