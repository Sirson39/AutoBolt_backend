using AutoBolt.Application.Interfaces;
using AutoBolt.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AutoBolt.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPartService, PartService>();
        services.AddScoped<ICustomerService, CustomerService>();
        
        // Other services will be registered here
        
        return services;
    }
}
