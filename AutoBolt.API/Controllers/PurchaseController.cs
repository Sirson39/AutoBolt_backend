using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;

namespace AutoBolt.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;

        public PurchaseController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PurchaseInvoiceDto>>> GetPurchaseInvoices()
        {
            var invoices = await _purchaseService.GetAllPurchaseInvoicesAsync();
            return Ok(invoices);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PurchaseInvoiceDto>> GetPurchaseInvoice(int id)
        {
            var invoice = await _purchaseService.GetPurchaseInvoiceByIdAsync(id);
            if (invoice == null) return NotFound();
            return Ok(invoice);
        }

        [HttpPost]
        public async Task<ActionResult<PurchaseInvoiceDto>> CreatePurchaseInvoice(CreatePurchaseInvoiceDto dto)
        {
            try
            {
                var createdInvoice = await _purchaseService.CreatePurchaseInvoiceAsync(dto);
                return CreatedAtAction(nameof(GetPurchaseInvoice), new { id = createdInvoice.Id }, createdInvoice);
            }
            catch (System.Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePurchaseInvoice(int id)
        {
            var success = await _purchaseService.DeletePurchaseInvoiceAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
