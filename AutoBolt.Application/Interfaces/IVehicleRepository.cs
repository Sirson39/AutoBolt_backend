using AutoBolt.Domain.Entities;

namespace AutoBolt.Application.Interfaces;

public interface IVehicleRepository : IGenericRepository<Vehicle>
{
    Task<IEnumerable<Vehicle>> GetVehiclesByCustomerIdAsync(int customerId);
}
