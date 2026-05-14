using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoBolt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Roles = "Admin,Staff")]
public class CustomersController(ICustomerService customerService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAll()
    {
        var customers = await customerService.GetAllCustomersAsync();
        return Ok(customers);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> Search([FromQuery] string query)
    {
        var customers = await customerService.SearchCustomersAsync(query);
        return Ok(customers);
    }

    [HttpGet("overdue-credits")]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetOverdueCredits()
    {
        var customers = await customerService.GetOverdueCreditCustomersAsync();
        return Ok(customers);
    }

    [HttpGet("{id}/history")]
    public async Task<ActionResult<CustomerHistoryDto>> GetHistory(int id)
    {
        var history = await customerService.GetCustomerHistoryAsync(id);
        if (history == null) return NotFound();
        return Ok(history);
    }

    [HttpGet("{id}/summary")]
    public async Task<ActionResult<CustomerSummaryDto>> GetSummary(int id)
    {
        var summary = await customerService.GetCustomerSummaryAsync(id);
        if (summary == null) return NotFound();
        return Ok(summary);
    }

    [HttpGet("{id}/timeline")]
    public async Task<ActionResult<CustomerTimelineDto>> GetTimeline(int id)
    {
        var timeline = await customerService.GetCustomerTimelineAsync(id);
        if (timeline == null) return NotFound();
        return Ok(timeline);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var customer = await customerService.GetCustomerByIdAsync(id);
        if (customer == null) return NotFound();
        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(CustomerCreateUpdateDto dto)
    {
        var createdCustomer = await customerService.CreateCustomerAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = createdCustomer.Id }, createdCustomer);
    }

    [HttpPost("register")]
    public async Task<ActionResult<CustomerRegistrationResultDto>> Register(CustomerRegistrationDto dto)
    {
        try
        {
            var result = await customerService.RegisterCustomerWithVehicleAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Customer.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/credit-payment")]
    public async Task<ActionResult<CustomerCreditPaymentResultDto>> ApplyCreditPayment(int id, CustomerCreditPaymentDto dto)
    {
        try
        {
            var result = await customerService.ApplyCreditPaymentAsync(id, dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CustomerCreateUpdateDto dto)
    {
        await customerService.UpdateCustomerAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await customerService.DeleteCustomerAsync(id);
        return NoContent();
    }
}
