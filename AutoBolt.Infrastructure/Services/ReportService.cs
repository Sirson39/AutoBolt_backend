using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoBolt.Infrastructure.Services;

public class ReportService(AutoBoltDbContext context) : IReportService
{
    public async Task<SalesReportDto> GetSalesReportAsync(string period)
    {
        var report = new SalesReportDto();
        
        var invoicesQuery = context.Invoices.AsQueryable();

        report.TotalRevenue = await invoicesQuery.SumAsync(i => i.TotalAmount);
        report.TotalOrders = await invoicesQuery.CountAsync();
        report.TotalItemsSold = await context.InvoiceItems.SumAsync(ii => ii.Quantity);

        if (period == "daily")
        {
            var startDate = DateTime.UtcNow.AddDays(-30);
            var trend = await invoicesQuery
                .Where(i => i.InvoiceDate >= startDate)
                .GroupBy(i => i.InvoiceDate.Date)
                .Select(g => new RevenuePointDto
                {
                    Label = g.Key.ToString("MMM dd"),
                    Revenue = g.Sum(i => i.TotalAmount),
                    OrderCount = g.Count()
                })
                .ToListAsync();
            report.RevenueTrend = trend.OrderBy(x => DateTime.ParseExact(x.Label, "MMM dd", null)).ToList();
        }
        else if (period == "monthly")
        {
            var year = DateTime.UtcNow.Year;
            var trend = await invoicesQuery
                .Where(i => i.InvoiceDate.Year == year)
                .GroupBy(i => i.InvoiceDate.Month)
                .Select(g => new RevenuePointDto
                {
                    Label = new DateTime(year, g.Key, 1).ToString("MMMM"),
                    Revenue = g.Sum(i => i.TotalAmount),
                    OrderCount = g.Count()
                })
                .ToListAsync();
            
            report.RevenueTrend = trend.OrderBy(x => DateTime.ParseExact(x.Label, "MMMM", null).Month).ToList();
        }
        else // yearly
        {
            var trend = await invoicesQuery
                .GroupBy(i => i.InvoiceDate.Year)
                .Select(g => new RevenuePointDto
                {
                    Label = g.Key.ToString(),
                    Revenue = g.Sum(i => i.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderBy(x => x.Label)
                .ToListAsync();
            report.RevenueTrend = trend;
        }

        report.TopParts = await context.InvoiceItems
            .Include(ii => ii.Part)
            .GroupBy(ii => ii.Part.Name)
            .Select(g => new TopPartDto
            {
                PartName = g.Key,
                QuantitySold = g.Sum(ii => ii.Quantity),
                TotalRevenue = g.Sum(ii => ii.SubTotal)
            })
            .OrderByDescending(x => x.QuantitySold)
            .Take(5)
            .ToListAsync();

        return report;
    }
}
