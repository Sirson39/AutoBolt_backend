using System;
using System.Collections.Generic;

namespace AutoBolt.Domain.Entities
{
    public class PurchaseInvoice : BaseEntity
    {
        public int VendorId { get; set; }
        public Vendor Vendor { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Remarks { get; set; }
        public List<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
    }
}
