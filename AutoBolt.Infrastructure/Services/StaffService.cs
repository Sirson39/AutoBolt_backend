using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Enums;
using AutoBolt.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace AutoBolt.Infrastructure.Services;

public class StaffService : IStaffService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;

    public StaffService(UserManager<ApplicationUser> userManager, IEmailService emailService)
    {
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
            throw new InvalidOperationException($"Failed to create staff: {errors}");
        }

        var roleName = dto.Role.ToString();
        await _userManager.AddToRoleAsync(user, roleName);

        // Email credentials to the new staff member. Failures to send email should
        // not break the staff creation flow — log and continue.
        try
        {
            await _emailService.SendStaffCredentialsAsync(dto.Email, dto.FullName, dto.Password);
        }
        catch (Exception ex)
        {
            // Keep creation successful even if email fails. Prefer logging.
            Console.WriteLine($"Warning: failed to send staff credentials email to {dto.Email}: {ex.Message}");
        }

        return MapToDto(user, dto.Role);
    }

    public async Task<bool> UpdateStaffAsync(int id, CreateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.UserName = dto.Email;
        user.PhoneNumber = dto.Phone;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded) return false;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, dto.Password);
        }

        // Update role if changed
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
            await _emailService.SendResendCredentialsAsync(user.Email!, user.FullName, newPassword);
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

        // Fisher-Yates shuffle so required chars are not always at positions 0-2
        for (int i = 11; i > 0; i--)
        {
            int j = bytes[i % bytes.Length] % (i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }

    private static UserDto MapToDto(ApplicationUser user, UserRole role)
    {
        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Phone = user.PhoneNumber ?? string.Empty,
            Role = role,
            IsActive = user.IsActive,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static UserRole ParseRole(string? roleName) =>
        roleName switch
        {
            "Admin" => UserRole.Admin,
            "Staff" => UserRole.Staff,
            _ => UserRole.Staff
        };
}
