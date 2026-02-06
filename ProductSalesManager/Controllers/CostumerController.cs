using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductSalesManager.Infrastructure.Data;
using ProductSalesManager.Domain.Entities;

namespace ProductSalesManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly AppDbContext _db;

    public CustomerController(AppDbContext db)
    {
        _db = db;
    }

    
    [HttpGet]
    public async Task<ActionResult<List<Customer>>> GetAll()
    {
        var customers = await _db.Customers
            .AsNoTracking()
            .OrderByDescending(c => c.Id)
            .ToListAsync();

        return Ok(customers);
    }

    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Customer>> GetById(int id)
    {
        var customer = await _db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer is null) return NotFound(new { message = "Cliente no encontrado." });

        return Ok(customer);
    }

    
    [HttpPost]
    public async Task<ActionResult<Customer>> Create([FromBody] Customer customer)
    {
        if (string.IsNullOrWhiteSpace(customer.Name))
            return BadRequest(new { message = "Name es requerido." });

        if (string.IsNullOrWhiteSpace(customer.Email))
            return BadRequest(new { message = "Email es requerido." });

        
        var emailExists = await _db.Customers.AnyAsync(c => c.Email == customer.Email);
        if (emailExists)
            return Conflict(new { message = "Ya existe un cliente con ese Email." });

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Customer customer)
    {
        if (id <= 0) return BadRequest(new { message = "Id inválido." });

        var existing = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (existing is null) return NotFound(new { message = "Cliente no encontrado." });

        if (string.IsNullOrWhiteSpace(customer.Name))
            return BadRequest(new { message = "Name es requerido." });

        if (string.IsNullOrWhiteSpace(customer.Email))
            return BadRequest(new { message = "Email es requerido." });

        
        if (!string.Equals(existing.Email, customer.Email, StringComparison.OrdinalIgnoreCase))
        {
            var emailExists = await _db.Customers.AnyAsync(c => c.Email == customer.Email);
            if (emailExists)
                return Conflict(new { message = "Ya existe un cliente con ese Email." });
        }

        existing.Name = customer.Name;
        existing.Email = customer.Email;
        existing.Phone = customer.Phone;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (existing is null) return NotFound(new { message = "Cliente no encontrado." });

        _db.Customers.Remove(existing);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
