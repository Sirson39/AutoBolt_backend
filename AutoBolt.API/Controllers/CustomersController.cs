using AutoBolt.Application.DTOs;
using AutoBolt.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AutoBolt.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
