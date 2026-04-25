using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using AutoBolt.Infrastructure.Data;

namespace AutoBolt.Infrastructure.Repositories;

public class VendorRepository(AutoBoltDbContext context) : GenericRepository<Vendor>(context), IVendorRepository
{
}
