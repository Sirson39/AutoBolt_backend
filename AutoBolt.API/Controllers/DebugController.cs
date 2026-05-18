using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBolt.API.Controllers;

[ApiController]
[Route("api/debug")]
public class DebugController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get()
    {
        var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
        var user = User.Identity?.IsAuthenticated == true ? "Authenticated" : "Not Authenticated";
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        
        return Ok(new {
            Headers = headers,
            AuthenticationStatus = user,
            Claims = claims
        });
    }
}
