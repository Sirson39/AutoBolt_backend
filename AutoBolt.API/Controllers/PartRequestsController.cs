using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBolt.API.Controllers;

[ApiController]
[Route("api/part-requests")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class PartRequestsController(IPartRequestService partRequestService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<PartRequestDto>>> GetAll()
    {
        return Ok(await partRequestService.GetAllAsync());
    }

    [HttpGet("customer/{customerId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<PartRequestDto>>> GetByCustomer(int customerId)
    {
        return Ok(await partRequestService.GetByCustomerIdAsync(customerId));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PartRequestDto>> Create(PartRequestCreateDto dto)
    {
        var result = await partRequestService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> UpdateStatus(int id, PartRequestUpdateStatusDto dto)
    {
        var result = await partRequestService.UpdateStatusAsync(id, dto.Status);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await partRequestService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
