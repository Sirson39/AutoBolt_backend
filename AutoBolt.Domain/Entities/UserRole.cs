namespace AutoBolt.Domain.Entities;

public class UserRole : BaseEntity
{
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public int RoleId { get; set; }
    public ApplicationRole Role { get; set; } = null!;
}
