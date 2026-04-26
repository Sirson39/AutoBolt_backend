using AutoBolt.Application.Features.Auth.Commands.ConfirmEmail;
using AutoBolt.Application.Features.Auth.Commands.Login;
using AutoBolt.Application.Features.Auth.Commands.RefreshToken;
using AutoBolt.Application.Features.Auth.Commands.RegisterCustomer;
using AutoBolt.Application.Features.Auth.Commands.RegisterStaff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBolt.API.Controllers;

public class AuthController : ApiControllerBase
{
    [HttpPost("register-customer")]
    public async Task<IActionResult> RegisterCustomer(RegisterCustomerCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("register-staff")]
    public async Task<IActionResult> RegisterStaff(RegisterStaffCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}
