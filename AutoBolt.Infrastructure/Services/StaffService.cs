using System.Net;
using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Enums;
using AutoBolt.Infrastructure.Data;
using AutoBolt.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Infrastructure.Services;

public class StaffService : IStaffService
{
    private readonly AutoBoltDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public StaffService(AutoBoltDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
    }

    public async Task<IEnumerable<UserDto>> GetAllStaffAsync()
    {
        var admins = await _userManager.GetUsersInRoleAsync("Admin");
        var staff = await _userManager.GetUsersInRoleAsync("Staff");

        return admins.Select(u => MapToDto(u, UserRole.Admin))
            .Concat(staff.Select(u => MapToDto(u, UserRole.Staff)))
            .OrderBy(u => u.FullName);
    }

    public async Task<UserDto?> GetStaffByIdAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = ParseRole(roles.FirstOrDefault());
        return MapToDto(user, role);
    }

    public async Task<UserDto> CreateStaffAsync(CreateUserDto dto)
    {
        if (!dto.Email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only @gmail.com addresses are allowed for staff registration.");

        if (dto.Phone != null && (dto.Phone.Length != 10 || !dto.Phone.All(char.IsDigit)))
            throw new InvalidOperationException("Phone number must be exactly 10 digits.");

        if (dto.Phone != null && await _context.Users.AnyAsync(u => u.PhoneNumber == dto.Phone))
            throw new InvalidOperationException("This phone number is already registered to another staff member.");

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            PhoneNumber = dto.Phone,
            IsActive = false
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create staff: {errors}");
        }

        await _userManager.AddToRoleAsync(user, dto.Role.ToString());

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebUtility.UrlEncode(token);
        var verifyLink = $"http://localhost:5173/#verify-email?userId={user.Id}&token={encodedToken}";

        try
        {
            await _emailService.SendEmailAsync(user.Email!, "Verify your AutoBolt Account", $@"
                <div style=""font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;"">
                    <h2 style=""color: #d95d39;"">Welcome to AutoBolt, {user.FullName}!</h2>
                    <p>An administrator has registered you on the AutoBolt Management System.</p>
                    <p>To finalize your account and set your permanent password, please click the button below:</p>
                    <div style=""text-align: center; margin: 30px 0;"">
                        <a href=""{verifyLink}"" style=""background: #d95d39; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;"">Setup My Account</a>
                    </div>
                    <p style=""font-size: 0.8rem; color: #666;"">If you didn't expect this, please ignore this email.</p>
                </div>
            ");
        }
        catch (Exception ex)
        {
            await _userManager.DeleteAsync(user);
            throw new InvalidOperationException($"Staff created but verification email failed: {ex.Message}.");
        }

        return MapToDto(user, dto.Role);
    }

    public async Task<bool> ConfirmAndSetupStaffAsync(int userId, string token, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        var confirmResult = await _userManager.ConfirmEmailAsync(user, token);
        if (!confirmResult.Succeeded) return false;

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var passwordResult = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
        if (!passwordResult.Succeeded) return false;

        user.IsActive = true;
        await _userManager.UpdateAsync(user);

        try
        {
            await _emailService.SendEmailAsync(user.Email!, "Account Activated - Welcome to AutoBolt", $@"
                <div style=""font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;"">
                    <h2 style=""color: #28a745;"">Account Activated!</h2>
                    <p>Hello {user.FullName}, your AutoBolt staff account is now fully active.</p>
                    <p>You can log in using your email and the password you just set.</p>
                    <div style=""text-align: center; margin: 30px 0;"">
                        <a href=""http://localhost:5173/#signin"" style=""background: #28a745; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;"">Go to Dashboard</a>
                    </div>
                </div>
            ");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: failed to send activation email to {user.Email}: {ex.Message}");
        }

        return true;
    }

    public async Task<bool> UpdateStaffAsync(int id, CreateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;

        if (user.EmailConfirmed && user.Email != dto.Email)
            throw new InvalidOperationException("Confirmed email addresses cannot be modified.");

        if (user.Email != dto.Email && !dto.Email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Only @gmail.com addresses are allowed.");

        if (dto.Phone != null && (dto.Phone.Length != 10 || !dto.Phone.All(char.IsDigit)))
            throw new InvalidOperationException("Phone number must be exactly 10 digits.");

        if (dto.Phone != null && user.PhoneNumber != dto.Phone && await _context.Users.AnyAsync(u => u.PhoneNumber == dto.Phone))
            throw new InvalidOperationException("Phone number already in use.");

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.UserName = dto.Email;
        user.PhoneNumber = dto.Phone;

        if (string.IsNullOrEmpty(user.SecurityStamp))
            await _userManager.UpdateSecurityStampAsync(user);

        if (!string.IsNullOrEmpty(dto.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, dto.Password);
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded) return false;

        var currentRoles = await _userManager.GetRolesAsync(user);
        var newRole = dto.Role.ToString();
        if (!currentRoles.Contains(newRole))
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, newRole);
        }

        return true;
    }

    public async Task<bool> DeleteStaffAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> ToggleStatusAsync(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;

        user.IsActive = !user.IsActive;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task ResendCredentialsAsync(int staffId)
    {
        var user = await _userManager.FindByIdAsync(staffId.ToString());
        if (user == null)
            throw new InvalidOperationException("Staff member not found.");

        var newPassword = GenerateSecurePassword();
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Password reset failed: {errors}");
        }

        try
        {
            await _emailService.SendEmailAsync(user.Email!, "AutoBolt — Your New Login Credentials", $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2563eb;'>New Credentials — AutoBolt</h2>
                    <p>Hi {user.FullName},</p>
                    <p>An administrator has reset your password. Here are your updated login credentials:</p>
                    <div style='background: #f3f4f6; padding: 16px; border-radius: 8px; margin: 16px 0;'>
                        <p><strong>Email:</strong> {user.Email}</p>
                        <p><strong>New Password:</strong> {newPassword}</p>
                    </div>
                    <p style='color: #ef4444;'>Please log in and change your password immediately.</p>
                    <p>Login at: <a href='http://localhost:5173'>AutoBolt Portal</a></p>
                    <hr/>
                    <p style='color: #6b7280; font-size: 12px;'>AutoBolt Vehicle Parts &amp; Service Management</p>
                </div>");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: failed to send resend-credentials email to {user.Email}: {ex.Message}");
        }
    }

    private static string GenerateSecurePassword()
    {
        const string digits = "23456789";
        const string upper  = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower  = "abcdefghjkmnpqrstuvwxyz";
        const string all    = digits + upper + lower;

        var bytes = new byte[12];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);

        var chars = new char[12];
        chars[0] = digits[bytes[0] % digits.Length];
        chars[1] = upper[bytes[1]  % upper.Length];
        chars[2] = lower[bytes[2]  % lower.Length];
        for (int i = 3; i < 12; i++)
            chars[i] = all[bytes[i] % all.Length];

        for (int i = 11; i > 0; i--)
        {
            int j = bytes[i % bytes.Length] % (i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }

    private static UserDto MapToDto(ApplicationUser user, UserRole role) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email ?? string.Empty,
        Phone = user.PhoneNumber ?? string.Empty,
        Role = role,
        IsActive = user.IsActive,
        CreatedAt = DateTime.UtcNow
    };

    private static UserRole ParseRole(string? roleName) =>
        roleName switch
        {
            "Admin" => UserRole.Admin,
            "Staff" => UserRole.Staff,
            _ => UserRole.Staff
        };
}
