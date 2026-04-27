namespace AutoBolt.Domain.Entities;

public class ApplicationUser : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    
    public bool EmailConfirmed { get; set; }
    public bool IsActive { get; set; } = true;

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    public string? EmailConfirmationOTP { get; set; }
    public DateTime? OTPExpiryTime { get; set; }

    // Relationships
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public Customer? CustomerDetails { get; set; }
}
