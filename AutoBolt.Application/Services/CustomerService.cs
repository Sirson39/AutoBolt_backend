using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using AutoBolt.Domain.Enums;

namespace AutoBolt.Application.Services;

public class CustomerService(
    ICustomerRepository customerRepository,
    IVehicleRepository vehicleRepository,
    IInvoiceRepository invoiceRepository) : ICustomerService
{
    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await customerRepository.GetAllAsync();
        return customers.Select(MapToDto);
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        var customer = await customerRepository.GetByIdAsync(id);
        return customer != null ? MapToDto(customer) : null;
    }

    public async Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllCustomersAsync();
        }

        var term = query.Trim();
        var customers = await customerRepository.GetAllAsync();
        var vehicles = await vehicleRepository.GetAllAsync();
        var matchedCustomerIds = new HashSet<int>();

        if (int.TryParse(term, out var numericTerm))
        {
            matchedCustomerIds.Add(numericTerm);
            foreach (var customer in customers.Where(customer => customer.Id == numericTerm))
            {
                matchedCustomerIds.Add(customer.Id);
            }
        }

        foreach (var vehicle in vehicles)
        {
            if (Contains(vehicle.LicensePlate, term) ||
                Contains(vehicle.VIN, term) ||
                vehicle.Id.ToString().Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                matchedCustomerIds.Add(vehicle.CustomerId);
            }
        }

        return customers
            .Where(customer =>
                Contains(customer.FullName, term) ||
                Contains(customer.Email, term) ||
                Contains(customer.Phone, term) ||
                Contains(customer.Address, term) ||
                customer.Id.ToString().Contains(term, StringComparison.OrdinalIgnoreCase) ||
                matchedCustomerIds.Contains(customer.Id))
            .Select(MapToDto);
    }

    public async Task<CustomerHistoryDto?> GetCustomerHistoryAsync(int id)
    {
        var customer = await customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            return null;
        }

        var vehicles = await vehicleRepository.GetVehiclesByCustomerIdAsync(id);
        var invoices = await invoiceRepository.GetAllWithDetailsAsync();

        return new CustomerHistoryDto
        {
            CustomerId = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            CreditBalance = customer.CreditBalance,
            Vehicles = vehicles.Select(vehicle => new CustomerVehicleHistoryDto
            {
                Id = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Mileage = vehicle.Mileage
            }).ToList(),
            Invoices = invoices
                .Where(invoice => invoice.CustomerId == id)
                .OrderByDescending(invoice => invoice.InvoiceDate)
                .Select(invoice => new CustomerInvoiceHistoryDto
                {
                    Id = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    InvoiceDate = invoice.InvoiceDate,
                    VehiclePlate = invoice.Vehicle?.LicensePlate ?? "N/A",
                    TotalAmount = invoice.TotalAmount,
                    Status = invoice.Status.ToString()
                })
                .ToList()
        };
    }

    public async Task<CustomerDto> CreateCustomerAsync(CustomerCreateUpdateDto dto)
    {
        var customer = new Customer
        {
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            CreditBalance = 0
        };

        await customerRepository.AddAsync(customer);
        await customerRepository.SaveChangesAsync();

        return MapToDto(customer);
    }

    public async Task<CustomerRegistrationResultDto> RegisterCustomerWithVehicleAsync(CustomerRegistrationDto dto)
    {
        var customers = await customerRepository.GetAllAsync();
        var duplicateCustomer = customers.FirstOrDefault(customer =>
            (!string.IsNullOrWhiteSpace(dto.Email) &&
             string.Equals(customer.Email, dto.Email.Trim(), StringComparison.OrdinalIgnoreCase)) ||
            string.Equals(customer.Phone, dto.Phone.Trim(), StringComparison.OrdinalIgnoreCase));

        if (duplicateCustomer != null)
        {
            throw new ArgumentException("A customer with the same phone number or email already exists.");
        }

        var vehicles = await vehicleRepository.GetAllAsync();
        if (vehicles.Any(vehicle =>
                string.Equals(vehicle.LicensePlate, dto.VehicleLicensePlate.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("A vehicle with the same registration number already exists.");
        }

        var customer = new Customer
        {
            FullName = dto.FullName.Trim(),
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            Phone = dto.Phone.Trim(),
            Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim(),
            CreditBalance = 0
        };

        await customerRepository.AddAsync(customer);
        await customerRepository.SaveChangesAsync();

        var vehicle = new Vehicle
        {
            LicensePlate = dto.VehicleLicensePlate.Trim(),
            Make = dto.VehicleMake.Trim(),
            Model = dto.VehicleModel.Trim(),
            Year = dto.VehicleYear,
            Mileage = dto.VehicleMileage,
            PlateType = (PlateType)dto.VehiclePlateType,
            CustomerId = customer.Id
        };

        await vehicleRepository.AddAsync(vehicle);
        await vehicleRepository.SaveChangesAsync();

        var savedVehicle = await vehicleRepository.GetByIdAsync(vehicle.Id);

        return new CustomerRegistrationResultDto
        {
            Customer = MapToDto(customer),
            Vehicle = MapToVehicleDto(savedVehicle ?? vehicle)
        };
    }

    public async Task UpdateCustomerAsync(int id, CustomerCreateUpdateDto dto)
    {
        var customer = await customerRepository.GetByIdAsync(id);
        if (customer == null) return;

        customer.FullName = dto.FullName;
        customer.Email = dto.Email;
        customer.Phone = dto.Phone;
        customer.Address = dto.Address;
        customer.UpdatedAt = DateTime.UtcNow;

        customerRepository.Update(customer);
        await customerRepository.SaveChangesAsync();
    }

    public async Task DeleteCustomerAsync(int id)
    {
        var customer = await customerRepository.GetByIdAsync(id);
        if (customer == null) return;

        customerRepository.Delete(customer);
        await customerRepository.SaveChangesAsync();
    }

    private static CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address,
            CreditBalance = customer.CreditBalance
        };
    }

    private static bool Contains(string? source, string term)
    {
        return !string.IsNullOrWhiteSpace(source) &&
               source.Contains(term, StringComparison.OrdinalIgnoreCase);
    }

    private static VehicleDto MapToVehicleDto(Vehicle vehicle)
    {
        return new VehicleDto
        {
            Id = vehicle.Id,
            LicensePlate = vehicle.LicensePlate,
            Make = vehicle.Make,
            Model = vehicle.Model,
            Year = vehicle.Year,
            Mileage = vehicle.Mileage,
            PlateType = (int)vehicle.PlateType,
            OwnerName = vehicle.Owner?.FullName ?? "Unknown"
        };
    }
}
