using System.Net;
using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using AutoBolt.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Infrastructure.Services;

public class StaffService(
    AutoBoltDbContext context,
    UserManager<User> userManager,
    IEmailService emailService) : IStaffService
{
    public async Task<IEnumerable<UserDto>> GetAllStaffAsync()
    {
        return await context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? "N/A",
                Phone = u.PhoneNumber ?? "N/A",
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<UserDto?> GetStaffByIdAsync(int id)
    {
        var u = await context.Users.FindAsync(id);
        if (u == null) return null;

        return new UserDto
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email ?? "N/A",
            Phone = u.PhoneNumber ?? "N/A",
            Role = u.Role,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        };
    }

    public async Task<UserDto> CreateStaffAsync(CreateUserDto dto)
    {
        // 1. Validate Email (Gmail only)
        if (!dto.Email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("Only @gmail.com addresses are allowed for staff registration.");
        }

        // 2. Validate Phone (10 digits)
        if (dto.Phone != null && (dto.Phone.Length != 10 || !dto.Phone.All(char.IsDigit)))
        {
            throw new Exception("Phone number must be exactly 10 digits.");
        }

        // 3. Check unique phone
        if (dto.Phone != null && await context.Users.AnyAsync(u => u.PhoneNumber == dto.Phone))
        {
            throw new Exception("This phone number is already registered to another staff member.");
        }

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.Phone,
            Role = dto.Role,
            IsActive = false
        };

        var result = await userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // Generate Verification Token
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebUtility.UrlEncode(token);
        
        // Build Verification Link (Assuming frontend URL)
        var verifyLink = $"http://localhost:5173/#verify-email?userId={user.Id}&token={encodedToken}";

        // Send Verification Email
        try
        {
            await emailService.SendEmailAsync(user.Email!, "Verify your AutoBolt Account", $@"
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
            // If email fails, delete the user so they can try again once SMTP is fixed
            await userManager.DeleteAsync(user);
            throw new Exception($"Staff created but verification email failed: {ex.Message}. Check your SMTP settings and restart the server.");
        }

        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.PhoneNumber ?? "N/A",
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> ConfirmAndSetupStaffAsync(int userId, string token, string newPassword)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null) return false;

        // 1. Confirm Email
        var confirmResult = await userManager.ConfirmEmailAsync(user, token);
        if (!confirmResult.Succeeded) return false;

        // 2. Set Password (Remove temporary one if it exists, or just reset)
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var passwordResult = await userManager.ResetPasswordAsync(user, resetToken, newPassword);
        if (!passwordResult.Succeeded) return false;

        // 3. Activate Account
        user.IsActive = true;
        await userManager.UpdateAsync(user);

        // 4. Send Welcome Email
        await emailService.SendEmailAsync(user.Email!, "Account Activated - Welcome to AutoBolt", $@"
            <div style=""font-family: sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;"">
                <h2 style=""color: #28a745;"">Success!</h2>
                <p>Hello {user.FullName}, your AutoBolt staff account is now fully active.</p>
                <p>You can now log in using your email and the password you just set.</p>
                <div style=""text-align: center; margin: 30px 0;"">
                    <a href=""http://localhost:5173/login"" style=""background: #28a745; color: white; padding: 12px 25px; text-decoration: none; border-radius: 5px; font-weight: bold;"">Go to Dashboard</a>
                </div>
                <p>Welcome to the team!</p>
            </div>
        ");

        return true;
    }

    public async Task<bool> UpdateStaffAsync(int id, CreateUserDto dto)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;

        // 1. Block email updates for active users
        if (user.EmailConfirmed && user.Email != dto.Email)
        {
            throw new Exception("Confirmed email addresses cannot be modified.");
        }

        // 2. Validate Email (Gmail only) if changed
        if (user.Email != dto.Email && !dto.Email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("Only @gmail.com addresses are allowed.");
        }

        // 3. Validate Phone (10 digits)
        if (dto.Phone != null && (dto.Phone.Length != 10 || !dto.Phone.All(char.IsDigit)))
        {
            throw new Exception("Phone number must be exactly 10 digits.");
        }

        // 4. Check unique phone if changed
        if (dto.Phone != null && user.PhoneNumber != dto.Phone && await context.Users.AnyAsync(u => u.PhoneNumber == dto.Phone))
        {
            throw new Exception("Phone number already in use.");
        }

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.UserName = dto.Email;
        user.PhoneNumber = dto.Phone;
        user.Role = dto.Role;
        
        // Ensure security stamp is present (prevents InvalidOperationException during update)
        if (string.IsNullOrEmpty(user.SecurityStamp))
        {
            await userManager.UpdateSecurityStampAsync(user);
        }
        
        if (!string.IsNullOrEmpty(dto.Password))
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            await userManager.ResetPasswordAsync(user, token, dto.Password);
        }

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> DeleteStaffAsync(int id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;

        await userManager.DeleteAsync(user);
        return true;
    }

    public async Task<bool> ToggleStatusAsync(int id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;

        user.IsActive = !user.IsActive;
        await userManager.UpdateAsync(user);
        return true;
    }
}
