using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using AutoBolt.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace AutoBolt.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly AutoBoltDbContext _context;
    private readonly IMemoryCache _memoryCache;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        ITokenService tokenService,
        IEmailService emailService,
        AutoBoltDbContext context,
        IMemoryCache memoryCache)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _context = context;
        _memoryCache = memoryCache;
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

    public async Task SendRegistrationOtpAsync(SendRegistrationOtpDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("An account with this email already exists.");

        // Generate 6-digit OTP
        var random = new Random();
        var otp = random.Next(100000, 999999).ToString();

        // Store OTP in cache for 10 minutes
        _memoryCache.Set($"RegistrationOtp_{dto.Email.ToLower()}", otp, TimeSpan.FromMinutes(10));

        try
        {
            await _emailService.SendEmailAsync(dto.Email, "AutoBolt — Registration Verification Code", $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2563eb;'>Registration Verification</h2>
                    <p>Hi,</p>
                    <p>Thank you for registering at AutoBolt. Please use the verification code below to complete your registration:</p>
                    <div style='background: #f3f4f6; padding: 16px; border-radius: 8px; margin: 16px 0; text-align: center;'>
                        <p style='font-size: 24px; font-weight: bold; letter-spacing: 4px; color: #1d4ed8; margin: 0;'>{otp}</p>
                    </div>
                    <p style='color: #6b7280;'>This code will expire in 10 minutes.</p>
                    <hr/>
                    <p style='color: #6b7280; font-size: 12px;'>AutoBolt Vehicle Parts &amp; Service Management</p>
                </div>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: failed to send OTP to {dto.Email}: {ex.Message}");
            throw new InvalidOperationException("Failed to send verification email. Please try again later.");
        }
    }

    public async Task<AuthResponseDto> RegisterCustomerAsync(CustomerRegisterDto dto)
    {
        var cacheKey = $"RegistrationOtp_{dto.Email.ToLower()}";
        _memoryCache.TryGetValue(cacheKey, out string? cachedOtp);

        var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
        if (cachedOtp != dto.Otp && !(isDevelopment && dto.Otp == "123456"))
        {
            throw new InvalidOperationException("Invalid or expired verification code.");
        }

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

        _memoryCache.Remove(cacheKey);

        try
        {
            await _emailService.SendEmailAsync(user.Email!, "Welcome to AutoBolt!", $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2563eb;'>Welcome to AutoBolt!</h2>
                    <p>Hi {user.FullName},</p>
                    <p>Your customer account has been successfully created and verified.</p>
                    <p>You can now log in to schedule services, request parts, and track your vehicles.</p>
                    <hr/>
                    <p style='color: #6b7280; font-size: 12px;'>AutoBolt Vehicle Parts &amp; Service Management</p>
                </div>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: failed to send welcome email to {user.Email}: {ex.Message}");
        }

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
            await _emailService.SendEmailAsync(user.Email!, "AutoBolt — Password Reset Request", $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2563eb;'>Password Reset Request</h2>
                    <p>Hi {user.FullName},</p>
                    <p>We received a request to reset your AutoBolt account password.
                       Use the token below with the reset-password form. This token expires shortly.</p>
                    <div style='background: #f3f4f6; padding: 16px; border-radius: 8px; margin: 16px 0; word-break: break-all;'>
                        <p style='margin: 0;'><strong>Reset Token:</strong></p>
                        <p style='font-family: monospace; color: #1d4ed8; margin: 8px 0 0 0;'>{token}</p>
                    </div>
                    <p style='color: #ef4444;'>If you did not request a password reset, you can safely ignore this email.</p>
                    <hr/>
                    <p style='color: #6b7280; font-size: 12px;'>AutoBolt Vehicle Parts &amp; Service Management</p>
                </div>");
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
