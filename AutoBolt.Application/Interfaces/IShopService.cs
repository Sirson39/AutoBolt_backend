using AutoBolt.Domain.Entities;

namespace AutoBolt.Application.Interfaces;

public interface IShopService
{
    Task<ShopConfiguration> GetConfigurationAsync();
    Task<bool> UpdateConfigurationAsync(ShopConfiguration config);
}
