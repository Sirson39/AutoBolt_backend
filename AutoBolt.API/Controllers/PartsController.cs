using AutoBolt.API.DTOs;
using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoBolt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartsController(IPartService partService, IWebHostEnvironment env) : ControllerBase
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

    private async Task<string?> SaveImageAsync(IFormFile? image)
    {
        if (image == null || image.Length == 0) return null;
        
        // Ensure web root exists, or fallback to ContentRoot/wwwroot if not explicitly set
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var uploadsFolder = Path.Combine(webRoot, "images", "parts");
        Directory.CreateDirectory(uploadsFolder);
        
        var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);
        
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(fileStream);
        }
        
        return "/images/parts/" + uniqueFileName;
    }

    [HttpPost]
    public async Task<ActionResult<PartDto>> Create([FromForm] PartCreateRequest request)
    {
        var dto = new PartCreateUpdateDto
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CategoryId = request.CategoryId,
            ImageUrl = await SaveImageAsync(request.Image)
        };
        
        var createdPart = await partService.CreatePartAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = createdPart.Id }, createdPart);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] PartCreateRequest request)
    {
        var dto = new PartCreateUpdateDto
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            CategoryId = request.CategoryId
        };
        
        // Only update ImageUrl if a new image was uploaded
        if (request.Image != null)
        {
            dto.ImageUrl = await SaveImageAsync(request.Image);
        }
        
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
