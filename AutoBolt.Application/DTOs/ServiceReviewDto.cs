using System.ComponentModel.DataAnnotations;

namespace AutoBolt.Application.DTOs;

public class ServiceReviewDto
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsPublic { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int? InvoiceId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ServiceReviewCreateDto
{
    [Required][Range(1, 5)] public int Rating { get; set; }
    public string? Comment { get; set; }
    [Required] public int CustomerId { get; set; }
    public int? InvoiceId { get; set; }
}
