using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBolt.API.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize]
public class VehiclePredictionController(IVehiclePredictionService predictionService) : ControllerBase
{
    [HttpGet("{id}/prediction")]
    public async Task<ActionResult<VehiclePredictionDto>> AnalyseVehicle(int id)
    {
        try
        {
            var result = await predictionService.AnalyseVehicleAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("customer/{customerId}/predictions")]
    public async Task<ActionResult<IEnumerable<VehiclePredictionDto>>> AnalyseCustomerVehicles(int customerId)
    {
        var results = await predictionService.AnalyseCustomerVehiclesAsync(customerId);
        return Ok(results);
    }
}
