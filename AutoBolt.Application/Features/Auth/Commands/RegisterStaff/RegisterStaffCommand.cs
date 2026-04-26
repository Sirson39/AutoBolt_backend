using AutoBolt.Application.Common.Interfaces;
using AutoBolt.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace AutoBolt.Application.Features.Auth.Commands.RegisterStaff;

public record RegisterStaffCommand(
    string FullName,
    string Email,
    string Password
) : IRequest<bool>;

public class RegisterStaffCommandHandler : IRequestHandler<RegisterStaffCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public RegisterStaffCommandHandler(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<bool> Handle(RegisterStaffCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new Exception("User with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            EmailConfirmed = true // Staff created by Admin are pre-confirmed (or you can send a reset link)
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new Exception($"Staff creation failed: {errors}");
        }

        if (!await _roleManager.RoleExistsAsync("Staff"))
        {
            await _roleManager.CreateAsync(new ApplicationRole("Staff"));
        }
        await _userManager.AddToRoleAsync(user, "Staff");

        return true;
    }
}
