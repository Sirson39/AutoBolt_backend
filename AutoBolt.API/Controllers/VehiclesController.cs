using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoBolt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController(IVehicleService vehicleService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAll()
    {
        var vehicles = await vehicleService.GetAllVehiclesAsync();
        return Ok(vehicles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VehicleDto>> GetById(int id)
    {
        var vehicle = await vehicleService.GetVehicleByIdAsync(id);
        if (vehicle == null) return NotFound();
        return Ok(vehicle);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetByCustomerId(int customerId)
    {
        var vehicles = await vehicleService.GetVehiclesByCustomerIdAsync(customerId);
        return Ok(vehicles);
    }

    [HttpPost]
    public async Task<ActionResult<VehicleDto>> Create(VehicleCreateUpdateDto dto)
    {
        try
        {
            var createdVehicle = await vehicleService.CreateVehicleAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdVehicle.Id }, createdVehicle);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, VehicleCreateUpdateDto dto)
    {
        var result = await vehicleService.UpdateVehicleAsync(id, dto);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await vehicleService.DeleteVehicleAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
