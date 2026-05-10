using AutoBolt.Application.DTOs;

namespace AutoBolt.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RegisterCustomerAsync(CustomerRegisterDto dto);
    Task ForgotPasswordAsync(ForgotPasswordDto dto);
    Task ResetPasswordAsync(ResetPasswordDto dto);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task<ProfileResponseDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
}
