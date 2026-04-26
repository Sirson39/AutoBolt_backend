using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using AutoBolt.Domain.Enums;

namespace AutoBolt.Application.Services;

public class InvoiceService(
    IInvoiceRepository invoiceRepository,
    IGenericRepository<Part> partRepository,
    IGenericRepository<Customer> customerRepository,
    IGenericRepository<Vehicle> vehicleRepository) : IInvoiceService
{
    public async Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync()
    {
        var invoices = await invoiceRepository.GetAllWithDetailsAsync();
        return invoices.Select(MapToDto);
    }

    public async Task<InvoiceDto?> GetInvoiceByIdAsync(int id)
    {
        var invoice = await invoiceRepository.GetByIdWithDetailsAsync(id);
        return invoice != null ? MapToDto(invoice) : null;
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(InvoiceCreateDto dto)
    {
        var invoice = new Invoice
        {
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
            InvoiceDate = DateTime.UtcNow,
            CustomerId = dto.CustomerId,
            VehicleId = dto.VehicleId,
            Status = (InvoiceStatus)dto.Status
        };

        decimal subTotal = 0;

        foreach (var itemDto in dto.Items)
        {
            var part = await partRepository.GetByIdAsync(itemDto.PartId);
            if (part == null) throw new Exception($"Part with ID {itemDto.PartId} not found.");

            if (part.StockQuantity < itemDto.Quantity)
                throw new Exception($"Insufficient stock for {part.Name}. Available: {part.StockQuantity}");

            var itemSubTotal = part.Price * itemDto.Quantity;
            subTotal += itemSubTotal;

            invoice.Items.Add(new InvoiceItem
            {
                PartId = itemDto.PartId,
                Quantity = itemDto.Quantity,
                UnitPrice = part.Price,
                SubTotal = itemSubTotal
            });

            // REDUCE STOCK
            part.StockQuantity -= itemDto.Quantity;
            partRepository.Update(part);
        }

        invoice.SubTotal = subTotal;

        // LOYALTY PROGRAM: 10% discount if spend > 5000
        if (subTotal > 5000)
        {
            invoice.DiscountAmount = subTotal * 0.10m;
        }
        else
        {
            invoice.DiscountAmount = 0;
        }

        invoice.TotalAmount = invoice.SubTotal - invoice.DiscountAmount;

        await invoiceRepository.AddAsync(invoice);
        await invoiceRepository.SaveChangesAsync();

        var createdInvoice = await invoiceRepository.GetByIdWithDetailsAsync(invoice.Id);
        return MapToDto(createdInvoice!);
    }

    public async Task<bool> CancelInvoiceAsync(int id)
    {
        var invoice = await invoiceRepository.GetByIdAsync(id);
        if (invoice == null || invoice.Status == InvoiceStatus.Cancelled) return false;

        invoice.Status = InvoiceStatus.Cancelled;
        
        // Logic to restock items if cancelled
        // (Optional for now, but good practice)
        
        invoiceRepository.Update(invoice);
        await invoiceRepository.SaveChangesAsync();
        return true;
    }

    private static InvoiceDto MapToDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            CustomerName = invoice.Customer?.FullName ?? $"Customer #{invoice.CustomerId}",
            VehiclePlate = invoice.Vehicle?.LicensePlate ?? "N/A",
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            TotalAmount = invoice.TotalAmount,
            Status = invoice.Status.ToString(),
            Items = invoice.Items.Select(ii => new InvoiceItemDto
            {
                PartId = ii.PartId,
                PartName = ii.Part?.Name ?? $"Part #{ii.PartId}",
                Quantity = ii.Quantity,
                UnitPrice = ii.UnitPrice,
                SubTotal = ii.SubTotal
            }).ToList()
        };
    }
}
