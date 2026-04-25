using AutoBolt.Application.Interfaces;
using AutoBolt.Infrastructure.Data;
using AutoBolt.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoBolt.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AutoBoltDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AutoBoltDbContext).Assembly.FullName)));

        // Repository registrations
        services.AddScoped<IPartRepository, PartRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        
        return services;
    }
}
