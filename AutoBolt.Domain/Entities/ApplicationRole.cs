using Microsoft.AspNetCore.Identity;

namespace AutoBolt.Domain.Entities;

public class ApplicationRole : IdentityRole<int>
{
    public ApplicationRole() : base() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
