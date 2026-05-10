using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using AutoBolt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Infrastructure.Services;

public class ShopService(AutoBoltDbContext context) : IShopService
{
    public async Task<ShopConfiguration> GetConfigurationAsync()
    {
        var config = await context.ShopConfigurations.FirstOrDefaultAsync();
        if (config == null)
        {
            config = new ShopConfiguration();
            context.ShopConfigurations.Add(config);
            await context.SaveChangesAsync();
        }
        return config;
    }

    public async Task<bool> UpdateConfigurationAsync(ShopConfiguration dto)
    {
        var config = await context.ShopConfigurations.FirstOrDefaultAsync();
        if (config == null)
        {
            config = new ShopConfiguration();
            context.ShopConfigurations.Add(config);
        }

        config.ShopName = dto.ShopName;
        config.Tagline = dto.Tagline;
        config.Address = dto.Address;
        config.LoyaltyThreshold = dto.LoyaltyThreshold;
        config.LoyaltyDiscountPercent = dto.LoyaltyDiscountPercent;

        context.ShopConfigurations.Update(config);
        return await context.SaveChangesAsync() > 0;
    }
}
