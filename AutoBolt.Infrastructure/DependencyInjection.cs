using System.Text;
using AutoBolt.Application.Interfaces;
using AutoBolt.Infrastructure.Data;
using AutoBolt.Infrastructure.Identity;
using AutoBolt.Infrastructure.Repositories;
using AutoBolt.Infrastructure.Services;
using AutoBolt.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace AutoBolt.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AutoBoltDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AutoBoltDbContext).Assembly.FullName)));

        // Memory Cache for OTPs
        services.AddMemoryCache();

        // ASP.NET Core Identity with int primary keys
        services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AutoBoltDbContext>()
        .AddDefaultTokenProviders();

        // JWT Authentication
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings section is missing from configuration.");

        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                ClockSkew = TimeSpan.Zero
            };
            options.IncludeErrorDetails = true;
        });

        // Email
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        // Repository registrations
        services.AddScoped<IPartRepository, PartRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Service registrations
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IStaffService, StaffService>();
        services.AddScoped<IPurchaseService, PurchaseService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IShopService, ShopService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IPartRequestService, PartRequestService>();
        services.AddScoped<IServiceReviewService, ServiceReviewService>();
        services.AddScoped<IVehiclePredictionService, VehiclePredictionService>();


        return services;
    }
}
