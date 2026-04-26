using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;

namespace AutoBolt.Application.Services;

public class VehicleService(IVehicleRepository vehicleRepository, ICustomerRepository customerRepository) : IVehicleService
{
    public async Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync()
    {
        var vehicles = await vehicleRepository.GetAllAsync();
        return vehicles.Select(MapToDto);
    }

    public async Task<VehicleDto?> GetVehicleByIdAsync(int id)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(id);
        return vehicle == null ? null : MapToDto(vehicle);
    }

    public async Task<IEnumerable<VehicleDto>> GetVehiclesByCustomerIdAsync(int customerId)
    {
        var vehicles = await vehicleRepository.GetVehiclesByCustomerIdAsync(customerId);
        return vehicles.Select(MapToDto);
    }

    public async Task<VehicleDto> CreateVehicleAsync(VehicleCreateUpdateDto dto)
    {
        // Optional: verify the customer exists before creation.
        var customerExists = await customerRepository.GetByIdAsync(dto.CustomerId) != null;
        if (!customerExists)
            throw new ArgumentException($"Customer with ID {dto.CustomerId} does not exist.");

        var vehicle = new Vehicle
        {
            LicensePlate = dto.LicensePlate,
            Make = dto.Make,
            Model = dto.Model,
            Year = dto.Year,
            Mileage = dto.Mileage,
            PlateType = (AutoBolt.Domain.Enums.PlateType)dto.PlateType,
            CustomerId = dto.CustomerId
        };

        await vehicleRepository.AddAsync(vehicle);
        await vehicleRepository.SaveChangesAsync();
        
        // Fetch it again to include the Owner for the DTO
        var fetchedVehicle = await vehicleRepository.GetByIdAsync(vehicle.Id);
        return MapToDto(fetchedVehicle ?? vehicle);
    }

    public async Task<bool> UpdateVehicleAsync(int id, VehicleCreateUpdateDto dto)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(id);
        if (vehicle == null) return false;

        vehicle.LicensePlate = dto.LicensePlate;
        vehicle.Make = dto.Make;
        vehicle.Model = dto.Model;
        vehicle.Year = dto.Year;
        vehicle.Mileage = dto.Mileage;
        vehicle.PlateType = (AutoBolt.Domain.Enums.PlateType)dto.PlateType;
        vehicle.CustomerId = dto.CustomerId;

        vehicleRepository.Update(vehicle);
        await vehicleRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteVehicleAsync(int id)
    {
        var vehicle = await vehicleRepository.GetByIdAsync(id);
        if (vehicle == null) return false;

        vehicleRepository.Delete(vehicle);
        await vehicleRepository.SaveChangesAsync();
        return true;
    }

    private static VehicleDto MapToDto(Vehicle vehicle)
    {
        return new VehicleDto
        {
            Id = vehicle.Id,
            LicensePlate = vehicle.LicensePlate,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Mileage = vehicle.Mileage,
            PlateType = (int)vehicle.PlateType,
            OwnerName = vehicle.Owner?.FullName ?? "Unknown"
        };
    }
}
