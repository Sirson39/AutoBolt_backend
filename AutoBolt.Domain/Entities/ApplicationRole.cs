using Microsoft.AspNetCore.Identity;

namespace AutoBolt.Domain.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() : base() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
