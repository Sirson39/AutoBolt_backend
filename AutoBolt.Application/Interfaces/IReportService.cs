using AutoBolt.Application.DTOs;

namespace AutoBolt.Application.Interfaces;

public interface IReportService
{
    Task<SalesReportDto> GetSalesReportAsync(string period); // "daily", "monthly", "yearly"
    Task<StaffReportsDto> GetStaffReportsAsync();
}
