using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBolt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Roles = "Admin")]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllStaff()
    {
        var staff = await _staffService.GetAllStaffAsync();
        return Ok(staff);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetStaff(int id)
    {
        var user = await _staffService.GetStaffByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateStaff(CreateUserDto dto)
    {
        try
        {
            var user = await _staffService.CreateStaffAsync(dto);
            return CreatedAtAction(nameof(GetStaff), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStaff(int id, CreateUserDto dto)
    {
        var result = await _staffService.UpdateStaffAsync(id, dto);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStaff(int id)
    {
        var result = await _staffService.DeleteStaffAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var result = await _staffService.ToggleStatusAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/resend-credentials")]
    public async Task<IActionResult> ResendCredentials(int id)
    {
        try
        {
            await _staffService.ResendCredentialsAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpPost("confirm-setup")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmSetup(ConfirmSetupDto dto)
    {
        var result = await _staffService.ConfirmAndSetupStaffAsync(dto.UserId, dto.Token, dto.NewPassword);
        if (!result) return BadRequest("Invalid or expired verification link.");
        return Ok(new { message = "Account activated successfully." });
    }
}

