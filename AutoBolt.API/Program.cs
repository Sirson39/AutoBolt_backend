using AutoBolt.Application;
using AutoBolt.Infrastructure;
using AutoBolt.Infrastructure.Data;
using AutoBolt.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    // Temporarily disable all authorization checks so the frontend can render data
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AllowAnonymousFilter());
});
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await SeedRolesAndAdminAsync(scope.ServiceProvider);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task SeedRolesAndAdminAsync(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var db = services.GetRequiredService<AutoBoltDbContext>();

    await db.Database.MigrateAsync();

    foreach (var role in new[] { "Admin", "Staff", "Customer" })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole<int>(role));
    }

    const string adminEmail = "admin@autobolt.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "System Admin",
            PhoneNumber = "0000000000",
            IsActive = true,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, "Admin@1234");
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}
