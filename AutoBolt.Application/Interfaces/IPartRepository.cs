using AutoBolt.Domain.Entities;

namespace AutoBolt.Application.Interfaces;

public interface IPartRepository : IGenericRepository<Part>
{
    Task<IEnumerable<Part>> GetLowStockPartsAsync(int threshold);
}
