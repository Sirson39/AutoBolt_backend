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
    private readonly IEmailService _emailService;
    private readonly AutoBoltDbContext _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        ITokenService tokenService,
        IEmailService emailService,
        AutoBoltDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _emailService = emailService;
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

    public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !user.IsActive)
            return; // Silent — never reveal whether the email is registered

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email!, user.FullName, token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: failed to send password reset email to {user.Email}: {ex.Message}");
        }
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !user.IsActive)
            throw new InvalidOperationException("Invalid request.");

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Password reset failed: {errors}");
        }
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !user.IsActive)
            throw new InvalidOperationException("User not found.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Password change failed: {errors}");
        }
    }

    public async Task<ProfileResponseDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null || !user.IsActive)
            throw new InvalidOperationException("User not found.");

        user.FullName = dto.FullName;
        if (dto.Phone != null)
            user.PhoneNumber = dto.Phone;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Profile update failed: {errors}");
        }

        // Mirror changes to Customer entity if one is linked (Customer-role users only)
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == userId);
        if (customer != null)
        {
            customer.FullName = dto.FullName;
            if (dto.Phone != null)
                customer.Phone = dto.Phone;
            await _context.SaveChangesAsync();
        }

        return new ProfileResponseDto
        {
            FullName = user.FullName,
            Email = user.Email!,
            Phone = user.PhoneNumber
        };
    }
}
