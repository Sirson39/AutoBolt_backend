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

    public async Task<StaffReportsDto> GetStaffReportsAsync()
    {
        var staffReports = new StaffReportsDto();
        var cutoffDate = DateTime.UtcNow.AddDays(-30);

        var invoices = await context.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .ToListAsync();

        var bookings = await context.Bookings
            .AsNoTracking()
            .Include(b => b.Customer)
            .ToListAsync();

        var customers = await context.Customers
            .AsNoTracking()
            .ToListAsync();

        var invoiceSummaries = invoices
            .Where(i => i.Status != Domain.Enums.InvoiceStatus.Cancelled)
            .GroupBy(i => new
            {
                i.CustomerId,
                CustomerName = i.Customer?.FullName ?? $"Customer #{i.CustomerId}",
                Email = i.Customer?.Email,
                Phone = i.Customer?.Phone ?? string.Empty
            })
            .Select(g => new
            {
                g.Key.CustomerId,
                g.Key.CustomerName,
                g.Key.Email,
                g.Key.Phone,
                TotalSpent = g.Sum(i => i.TotalAmount),
                Orders = g.Count(),
                InvoiceCount = g.Count(),
                OldestPendingInvoiceDate = g.Where(i => i.Status == Domain.Enums.InvoiceStatus.Pending)
                    .Select(i => (DateTime?)i.InvoiceDate)
                    .Min(),
                OutstandingAmount = g.Where(i => i.Status == Domain.Enums.InvoiceStatus.Pending)
                    .Sum(i => (decimal?)i.TotalAmount) ?? 0m
            })
            .ToList();

        var bookingLookup = bookings
            .Where(b => b.Status != Domain.Enums.BookingStatus.Cancelled)
            .GroupBy(b => b.CustomerId)
            .ToDictionary(g => g.Key, g => g.Count());

        staffReports.TopSpenders = invoiceSummaries
            .OrderByDescending(x => x.TotalSpent)
            .Take(5)
            .Select(x => new TopSpenderDto
            {
                CustomerId = x.CustomerId,
                CustomerName = x.CustomerName,
                Email = x.Email,
                Phone = x.Phone,
                TotalSpent = x.TotalSpent,
                Orders = x.Orders
            })
            .ToList();

        staffReports.Regulars = invoiceSummaries
            .Select(x =>
            {
                bookingLookup.TryGetValue(x.CustomerId, out var bookingCount);
                var visitCount = x.InvoiceCount + bookingCount;
                return new RegularCustomerDto
                {
                    CustomerId = x.CustomerId,
                    CustomerName = x.CustomerName,
                    Email = x.Email,
                    Phone = x.Phone,
                    InvoiceCount = x.InvoiceCount,
                    BookingCount = bookingCount,
                    VisitCount = visitCount
                };
            })
            .OrderByDescending(x => x.VisitCount)
            .ThenByDescending(x => x.InvoiceCount)
            .Take(5)
            .ToList();

        staffReports.PendingCredits = invoiceSummaries
            .Where(x => x.OldestPendingInvoiceDate.HasValue && x.OldestPendingInvoiceDate.Value <= cutoffDate && x.OutstandingAmount > 0)
            .OrderByDescending(x => x.OutstandingAmount)
            .Select(x => new PendingCreditDto
            {
                CustomerId = x.CustomerId,
                CustomerName = x.CustomerName,
                Email = x.Email,
                Phone = x.Phone,
                CreditBalance = customers.FirstOrDefault(c => c.Id == x.CustomerId)?.CreditBalance ?? 0m,
                OutstandingAmount = x.OutstandingAmount,
                OldestPendingInvoiceDate = x.OldestPendingInvoiceDate!.Value,
                DaysOutstanding = (int)(DateTime.UtcNow - x.OldestPendingInvoiceDate!.Value).TotalDays
            })
            .ToList();

        return staffReports;
    }
}
