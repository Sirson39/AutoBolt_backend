using AutoBolt.Domain.Enums;

namespace AutoBolt.Application.DTOs;

public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = null!;
    public DateTime InvoiceDate { get; set; }
    public string CustomerName { get; set; } = null!;
    public string? VehiclePlate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = null!;
    public List<InvoiceItemDto> Items { get; set; } = new();
}

public class InvoiceItemDto
{
    public int PartId { get; set; }
    public string PartName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal SubTotal { get; set; }
}

public class InvoiceCreateDto
{
    public int CustomerId { get; set; }
    public int? VehicleId { get; set; }
    public int Status { get; set; } // Add this
    public List<InvoiceItemCreateDto> Items { get; set; } = new();
}

public class InvoiceItemCreateDto
{
    public int PartId { get; set; }
    public int Quantity { get; set; }
}
