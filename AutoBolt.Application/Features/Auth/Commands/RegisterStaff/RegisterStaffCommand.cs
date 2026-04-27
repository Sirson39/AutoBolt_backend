using System.ComponentModel.DataAnnotations;
using AutoBolt.Application.Common.Interfaces;
using AutoBolt.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Application.Features.Auth.Commands.RegisterStaff;

public record RegisterStaffCommand : IRequest<bool>
{
    [Required(ErrorMessage = "Full name is required.")]
    [MaxLength(100, ErrorMessage = "Full name must not exceed 100 characters.")]
    public string FullName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "A valid email address is required.")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string Password { get; init; } = string.Empty;
}

public class RegisterStaffCommandHandler : IRequestHandler<RegisterStaffCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

    public RegisterStaffCommandHandler(
        IApplicationDbContext context,
        IPasswordHasher<ApplicationUser> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<bool> Handle(RegisterStaffCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (existingUser)
        {
            throw new Exception("User with this email already exists.");
        }

        var user = new ApplicationUser
        {
            Email = request.Email,
            FullName = request.FullName,
            EmailConfirmed = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        var staffRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Staff", cancellationToken);
        if (staffRole == null)
        {
            staffRole = new ApplicationRole("Staff");
            _context.Roles.Add(staffRole);
            await _context.SaveChangesAsync(cancellationToken);
        }

        user.UserRoles.Add(new UserRole { User = user, Role = staffRole });

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
