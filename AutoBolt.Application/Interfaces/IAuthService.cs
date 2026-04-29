using AutoBolt.Application.DTOs;

namespace AutoBolt.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> RegisterCustomerAsync(CustomerRegisterDto dto);
}
