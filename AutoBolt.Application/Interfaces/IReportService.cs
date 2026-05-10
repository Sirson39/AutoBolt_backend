using AutoBolt.Application.DTOs;

namespace AutoBolt.Application.Interfaces;

public interface IReportService
{
    Task<SalesReportDto> GetSalesReportAsync(string period);
    Task<StaffReportsDto> GetStaffReportsAsync();
}
