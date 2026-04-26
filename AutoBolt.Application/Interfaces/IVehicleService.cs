using AutoBolt.Application.DTOs;

namespace AutoBolt.Application.Interfaces;

public interface IVehicleService
{
    Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync();
    Task<VehicleDto?> GetVehicleByIdAsync(int id);
    Task<IEnumerable<VehicleDto>> GetVehiclesByCustomerIdAsync(int customerId);
    Task<VehicleDto> CreateVehicleAsync(VehicleCreateUpdateDto dto);
    Task<bool> UpdateVehicleAsync(int id, VehicleCreateUpdateDto dto);
    Task<bool> DeleteVehicleAsync(int id);
}
