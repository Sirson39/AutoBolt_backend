using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using AutoBolt.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly AutoBoltDbContext _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        ITokenService tokenService,
        AutoBoltDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _context = context;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !user.IsActive)
            return null;

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? string.Empty;

        // For customers, include the linked customerId in the token
        int? customerId = null;
        if (role == "Customer")
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id);
            customerId = customer?.Id;
        }

        var expiry = DateTime.UtcNow.AddMinutes(60);
        var token = _tokenService.GenerateToken(user.Id, user.Email!, user.FullName, role, customerId);

        return new AuthResponseDto
        {
            Token = token,
            Role = role,
            FullName = user.FullName,
            Email = user.Email!,
            Expiry = expiry
        };
    }

    public async Task<AuthResponseDto> RegisterCustomerAsync(CustomerRegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("An account with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.Phone,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, "Customer");

        // Create linked Customer entity
        var customer = new Customer
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            CreditBalance = 0,
            UserId = user.Id
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user.Id, user.Email!, user.FullName, "Customer", customer.Id);

        return new AuthResponseDto
        {
            Token = token,
            Role = "Customer",
            FullName = user.FullName,
            Email = user.Email!,
            Expiry = DateTime.UtcNow.AddMinutes(60)
        };
    }
}
