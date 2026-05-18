using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBolt.API.Controllers;

[ApiController]
[Route("api/reviews")]
public class ServiceReviewsController(IServiceReviewService reviewService) : ControllerBase
{
    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "Admin,Staff")]
    public async Task<ActionResult<IEnumerable<ServiceReviewDto>>> GetAll()
    {
        return Ok(await reviewService.GetAllAsync());
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ServiceReviewDto>>> GetPublic()
    {
        return Ok(await reviewService.GetPublicAsync());
    }

    [HttpGet("customer/{customerId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ServiceReviewDto>>> GetByCustomer(int customerId)
    {
        return Ok(await reviewService.GetByCustomerIdAsync(customerId));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ServiceReviewDto>> Create(ServiceReviewCreateDto dto)
    {
        var result = await reviewService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), new { }, result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await reviewService.DeleteAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }
}
