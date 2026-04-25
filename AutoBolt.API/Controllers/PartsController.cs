using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoBolt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartsController(IPartService partService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PartDto>>> GetAll()
    {
        var parts = await partService.GetAllPartsAsync();
        return Ok(parts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PartDto>> GetById(int id)
    {
        var part = await partService.GetPartByIdAsync(id);
        if (part == null) return NotFound();
        return Ok(part);
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<PartDto>>> GetLowStock()
    {
        var parts = await partService.GetLowStockPartsAsync();
        return Ok(parts);
    }

    [HttpPost]
    public async Task<ActionResult<PartDto>> Create(PartCreateUpdateDto dto)
    {
        var createdPart = await partService.CreatePartAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = createdPart.Id }, createdPart);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, PartCreateUpdateDto dto)
    {
        await partService.UpdatePartAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await partService.DeletePartAsync(id);
        return NoContent();
    }
}
