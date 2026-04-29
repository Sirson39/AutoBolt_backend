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

        // Email credentials to the new staff member
        await _emailService.SendStaffCredentialsAsync(dto.Email, dto.FullName, dto.Password);

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
