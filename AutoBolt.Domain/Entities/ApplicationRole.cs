namespace AutoBolt.Domain.Entities;

public class ApplicationRole : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ApplicationRole() { }
    public ApplicationRole(string name)
    {
        Name = name;
    }
}
