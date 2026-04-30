using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoBolt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController(IInvoiceService invoiceService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetAll()
    {
        var invoices = await invoiceService.GetAllInvoicesAsync();
        return Ok(invoices);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InvoiceDto>> GetById(int id)
    {
        var invoice = await invoiceService.GetInvoiceByIdAsync(id);
        if (invoice == null) return NotFound();
        return Ok(invoice);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> Create(InvoiceCreateDto dto)
    {
        try 
        {
            var createdInvoice = await invoiceService.CreateInvoiceAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdInvoice.Id }, createdInvoice);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<InvoiceDto>> Checkout(InvoiceCreateDto dto)
    {
        return await Create(dto);
    }

    [HttpDelete("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await invoiceService.CancelInvoiceAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/email")]
    public async Task<IActionResult> EmailInvoice(int id, [FromQuery] string? recipientEmail = null)
    {
        try
        {
            var result = await invoiceService.EmailInvoiceAsync(id, recipientEmail);
            if (!result) return NotFound();
            return Ok(new { message = "Invoice email sent successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
