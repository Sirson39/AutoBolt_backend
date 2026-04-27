using AutoBolt.Application.Common.Interfaces;
using AutoBolt.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Infrastructure.Data;

public class AutoBoltDbContext : DbContext, IApplicationDbContext
{
    public AutoBoltDbContext(DbContextOptions<AutoBoltDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> Users { get; set; }
    public DbSet<ApplicationRole> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    
    public DbSet<Part> Parts { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User-Role Many-to-Many
        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.CustomerDetails)
            .WithOne(c => c.User)
            .HasForeignKey<Customer>(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InvoiceItem>()
            .HasOne(ii => ii.Invoice)
            .WithMany(i => i.Items)
            .HasForeignKey(ii => ii.InvoiceId);

        modelBuilder.Entity<Vehicle>()
            .HasOne(v => v.Owner)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.CustomerId);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Customer)
            .WithMany(c => c.Bookings)
            .HasForeignKey(b => b.CustomerId);
            
        // Decimal precision configurations
        modelBuilder.Entity<Part>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Customer>()
            .Property(c => c.CreditBalance)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.DiscountAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.FinalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<InvoiceItem>()
            .Property(ii => ii.UnitPrice)
            .HasPrecision(18, 2);
    }
}
