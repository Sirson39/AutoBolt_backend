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
