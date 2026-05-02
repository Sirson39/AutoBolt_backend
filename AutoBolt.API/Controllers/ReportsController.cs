using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoBolt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("sales")]
    public async Task<ActionResult<SalesReportDto>> GetSalesReport([FromQuery] string period = "daily")
    {
        var report = await reportService.GetSalesReportAsync(period.ToLower());
        return Ok(report);
    }

    [HttpGet("staff")]
    public async Task<ActionResult<StaffReportsDto>> GetStaffReports()
    {
        var report = await reportService.GetStaffReportsAsync();
        return Ok(report);
    }
}
