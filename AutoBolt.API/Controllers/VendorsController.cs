using AutoBolt.API.DTOs;
using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoBolt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendorsController(IVendorService vendorService, IWebHostEnvironment env) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VendorDto>>> GetAll()
    {
        var vendors = await vendorService.GetAllVendorsAsync();
        return Ok(vendors);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VendorDto>> GetById(int id)
    {
        var vendor = await vendorService.GetVendorByIdAsync(id);
        if (vendor == null) return NotFound();
        return Ok(vendor);
    }

    private async Task<string?> SaveLogoAsync(IFormFile? logo)
    {
        if (logo == null || logo.Length == 0) return null;
        
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var uploadsFolder = Path.Combine(webRoot, "images", "vendors");
        Directory.CreateDirectory(uploadsFolder);
        
        var uniqueFileName = Guid.NewGuid().ToString() + "_" + logo.FileName;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
        
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await logo.CopyToAsync(fileStream);
        }
        
        return "/images/vendors/" + uniqueFileName;
    }

    [HttpPost]
    public async Task<ActionResult<VendorDto>> Create([FromForm] VendorCreateRequest request)
    {
        var dto = new VendorCreateUpdateDto
        {
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            LogoUrl = await SaveLogoAsync(request.Logo)
        };
        
        var createdVendor = await vendorService.CreateVendorAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = createdVendor.Id }, createdVendor);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] VendorCreateRequest request)
    {
        var dto = new VendorCreateUpdateDto
        {
            Name = request.Name,
            ContactPerson = request.ContactPerson,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address
        };
        
        if (request.Logo != null)
        {
            dto.LogoUrl = await SaveLogoAsync(request.Logo);
        }
        
        await vendorService.UpdateVendorAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await vendorService.DeleteVendorAsync(id);
        return NoContent();
    }
}
