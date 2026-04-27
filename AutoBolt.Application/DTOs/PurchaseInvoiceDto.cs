using System;
using System.Collections.Generic;

namespace AutoBolt.Application.DTOs
{
    public class PurchaseInvoiceDto
    {
        public int Id { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Remarks { get; set; }
        public List<PurchaseInvoiceItemDto> Items { get; set; } = new List<PurchaseInvoiceItemDto>();
    }

    public class PurchaseInvoiceItemDto
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public string PartName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class CreatePurchaseInvoiceDto
    {
        public int VendorId { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Remarks { get; set; }
        public List<CreatePurchaseInvoiceItemDto> Items { get; set; } = new List<CreatePurchaseInvoiceItemDto>();
    }

    public class CreatePurchaseInvoiceItemDto
    {
        public int PartId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
    }
}
