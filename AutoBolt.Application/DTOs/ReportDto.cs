using System;
using System.Collections.Generic;

namespace AutoBolt.Application.DTOs;

public class SalesReportDto
{
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalItemsSold { get; set; }
    public decimal AverageOrderValue => TotalOrders > 0 ? TotalRevenue / TotalOrders : 0;
    public List<RevenuePointDto> RevenueTrend { get; set; } = new();
    public List<TopPartDto> TopParts { get; set; } = new();
}

public class RevenuePointDto
{
    public string Label { get; set; } // "2026-04-25", "April", "2026"
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class TopPartDto
{
    public string PartName { get; set; }
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class StaffReportsDto
{
    public List<TopSpenderDto> TopSpenders { get; set; } = new();
    public List<RegularCustomerDto> Regulars { get; set; } = new();
    public List<PendingCreditDto> PendingCredits { get; set; } = new();
}

public class TopSpenderDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public int Orders { get; set; }
}

public class RegularCustomerDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public int VisitCount { get; set; }
    public int InvoiceCount { get; set; }
    public int BookingCount { get; set; }
}

public class PendingCreditDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Phone { get; set; } = string.Empty;
    public decimal CreditBalance { get; set; }
    public decimal OutstandingAmount { get; set; }
    public DateTime OldestPendingInvoiceDate { get; set; }
    public int DaysOutstanding { get; set; }
}
