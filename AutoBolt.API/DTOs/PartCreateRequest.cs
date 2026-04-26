using System.ComponentModel.DataAnnotations;

namespace AutoBolt.API.DTOs;

public class PartCreateRequest
{
    [Required]
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    [Required]
    public decimal Price { get; set; }
    [Required]
    public int StockQuantity { get; set; }
    [Required]
    public int CategoryId { get; set; }
    public IFormFile? Image { get; set; }
}
