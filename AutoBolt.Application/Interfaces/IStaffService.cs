using AutoBolt.Application.DTOs;

namespace AutoBolt.Application.Interfaces;

public interface IStaffService
{
    Task<IEnumerable<UserDto>> GetAllStaffAsync();
    Task<UserDto?> GetStaffByIdAsync(int id);
    Task<UserDto> CreateStaffAsync(CreateUserDto createUserDto);
    Task<bool> UpdateStaffAsync(int id, CreateUserDto updateUserDto);
    Task<bool> DeleteStaffAsync(int id);
    Task<bool> ToggleStatusAsync(int id);
}
