using AutoBolt.Application.DTOs;

namespace AutoBolt.Application.Interfaces;

public interface IVehiclePredictionService
{
    Task<VehiclePredictionDto> AnalyseVehicleAsync(int vehicleId);
    Task<IEnumerable<VehiclePredictionDto>> AnalyseCustomerVehiclesAsync(int customerId);
}
