using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AutoBolt.API.Controllers;

[ApiController]
[Route("api/config")]
public class ConfigController(IShopService shopService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetConfig()
    {
        return Ok(await shopService.GetConfigurationAsync());
    }

    [HttpPut]
    public async Task<IActionResult> UpdateConfig(ShopConfiguration config)
    {
        var result = await shopService.UpdateConfigurationAsync(config);
        if (!result) return BadRequest("Failed to update configuration.");
        return NoContent();
    }
}
