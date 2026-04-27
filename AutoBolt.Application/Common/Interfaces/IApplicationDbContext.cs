using AutoBolt.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<ApplicationUser> Users { get; }
    DbSet<ApplicationRole> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    
    DbSet<Part> Parts { get; }
    DbSet<Vehicle> Vehicles { get; }
    DbSet<Vendor> Vendors { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceItem> InvoiceItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
