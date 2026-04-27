using AutoBolt.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AutoBolt.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AutoBoltDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<ApplicationUser>>();

        // Ensure database is created
        await context.Database.MigrateAsync();

        // Seed Roles
        string[] roles = { "Admin", "Staff", "Customer" };
        foreach (var roleName in roles)
        {
            if (!await context.Roles.AnyAsync(r => r.Name == roleName))
            {
                context.Roles.Add(new ApplicationRole(roleName));
            }
        }
        await context.SaveChangesAsync();

        // Seed Admin User
        var adminEmail = "admin@autobolt.com";
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                Email = adminEmail,
                FullName = "System Admin",
                EmailConfirmed = true,
                IsActive = true
            };

            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123");
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();

            var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
            context.UserRoles.Add(new UserRole { User = adminUser, Role = adminRole });
            await context.SaveChangesAsync();
        }
    }
}
