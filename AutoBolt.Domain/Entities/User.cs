using AutoBolt.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace AutoBolt.Domain.Entities;

public class User : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // IdentityUser<int> already provides:
    // public string Email { get; set; }
    // public string PasswordHash { get; set; }
    // public string PhoneNumber { get; set; }
}
