using System.ComponentModel.DataAnnotations;
using AutoBolt.Domain.Enums;

namespace AutoBolt.Application.DTOs;

public class PartRequestDto
{
    public int Id { get; set; }
    public string PartName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PartRequestCreateDto
{
    [Required] public string PartName { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Range(1, 100)] public int Quantity { get; set; } = 1;
    [Required] public int CustomerId { get; set; }
}

public class PartRequestUpdateStatusDto
{
    [Required] public PartRequestStatus Status { get; set; }
}
