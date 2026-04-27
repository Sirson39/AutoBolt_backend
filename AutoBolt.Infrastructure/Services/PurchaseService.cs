using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using AutoBolt.Domain.Entities;
using AutoBolt.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoBolt.Infrastructure.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly AutoBoltDbContext _context;

        public PurchaseService(AutoBoltDbContext context)
        {
            _context = context;
        }

        public async Task<List<PurchaseInvoiceDto>> GetAllPurchaseInvoicesAsync()
        {
            return await _context.PurchaseInvoices
                .Include(pi => pi.Vendor)
                .Include(pi => pi.Items)
                    .ThenInclude(i => i.Part)
                .OrderByDescending(pi => pi.PurchaseDate)
                .Select(pi => new PurchaseInvoiceDto
                {
                    Id = pi.Id,
                    VendorId = pi.VendorId,
                    VendorName = pi.Vendor.Name,
                    InvoiceNumber = pi.InvoiceNumber,
                    PurchaseDate = pi.PurchaseDate,
                    TotalAmount = pi.TotalAmount,
                    Remarks = pi.Remarks,
                    Items = pi.Items.Select(i => new PurchaseInvoiceItemDto
                    {
                        Id = i.Id,
                        PartId = i.PartId,
                        PartName = i.Part.Name,
                        Quantity = i.Quantity,
                        UnitCost = i.UnitCost,
                        Subtotal = i.Subtotal
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<PurchaseInvoiceDto> GetPurchaseInvoiceByIdAsync(int id)
        {
            var pi = await _context.PurchaseInvoices
                .Include(p => p.Vendor)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Part)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pi == null) return null;

            return new PurchaseInvoiceDto
            {
                Id = pi.Id,
                VendorId = pi.VendorId,
                VendorName = pi.Vendor.Name,
                InvoiceNumber = pi.InvoiceNumber,
                PurchaseDate = pi.PurchaseDate,
                TotalAmount = pi.TotalAmount,
                Remarks = pi.Remarks,
                Items = pi.Items.Select(i => new PurchaseInvoiceItemDto
                {
                    Id = i.Id,
                    PartId = i.PartId,
                    PartName = i.Part.Name,
                    Quantity = i.Quantity,
                    UnitCost = i.UnitCost,
                    Subtotal = i.Subtotal
                }).ToList()
            };
        }

        public async Task<PurchaseInvoiceDto> CreatePurchaseInvoiceAsync(CreatePurchaseInvoiceDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var purchaseInvoice = new PurchaseInvoice
                {
                    VendorId = dto.VendorId,
                    // Auto-generate invoice number like Sales
                    InvoiceNumber = $"PUR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                    // Ensure UTC for Postgres
                    PurchaseDate = DateTime.SpecifyKind(dto.PurchaseDate, DateTimeKind.Utc),
                    Remarks = dto.Remarks,
                    CreatedAt = DateTime.UtcNow
                };

                decimal totalAmount = 0;

                foreach (var itemDto in dto.Items)
                {
                    var part = await _context.Parts.FindAsync(itemDto.PartId);
                    if (part == null) continue;

                    var subtotal = itemDto.Quantity * itemDto.UnitCost;
                    totalAmount += subtotal;

                    purchaseInvoice.Items.Add(new PurchaseInvoiceItem
                    {
                        PartId = itemDto.PartId,
                        Quantity = itemDto.Quantity,
                        UnitCost = itemDto.UnitCost,
                        Subtotal = subtotal,
                        CreatedAt = DateTime.UtcNow
                    });

                    // Update stock quantity
                    part.StockQuantity += itemDto.Quantity;
                }

                purchaseInvoice.TotalAmount = totalAmount;

                _context.PurchaseInvoices.Add(purchaseInvoice);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetPurchaseInvoiceByIdAsync(purchaseInvoice.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Log or throw specific message
                throw new Exception($"Failed to save purchase: {ex.Message} {ex.InnerException?.Message}");
            }
        }

        public async Task<bool> DeletePurchaseInvoiceAsync(int id)
        {
            var invoice = await _context.PurchaseInvoices
                .Include(pi => pi.Items)
                .FirstOrDefaultAsync(pi => pi.Id == id);

            if (invoice == null) return false;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Optionally: Rollback stock when deleting an invoice? 
                // Usually, we don't delete invoices, but if we do, we should handle stock.
                foreach (var item in invoice.Items)
                {
                    var part = await _context.Parts.FindAsync(item.PartId);
                    if (part != null)
                    {
                        part.StockQuantity -= item.Quantity;
                    }
                }

                _context.PurchaseInvoices.Remove(invoice);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }
    }
}
