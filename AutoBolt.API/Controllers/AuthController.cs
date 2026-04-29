using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBolt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var response = await authService.LoginAsync(dto);
        if (response == null)
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(response);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> RegisterCustomer(CustomerRegisterDto dto)
    {
        try
        {
            var response = await authService.RegisterCustomerAsync(dto);
            return CreatedAtAction(nameof(Login), response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public ActionResult<object> GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                 ?? User.FindFirst("email")?.Value;
        var name = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                ?? User.FindFirst("name")?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var customerId = User.FindFirst("customerId")?.Value;

        return Ok(new
        {
            UserId = userId,
            Email = email,
            FullName = name,
            Role = role,
            CustomerId = customerId
        });
    }
}
