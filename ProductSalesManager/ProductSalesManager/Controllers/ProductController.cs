using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductSalesManager.Domain.Entities;
using ProductSalesManager.Infrastructure.Data;

namespace ProductSalesManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    
    [HttpGet]
    public async Task<ActionResult<List<Product>>> GetAll()
    {
        var products = await _db.Products
            .AsNoTracking()
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        return Ok(products);
    }

    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null) return NotFound(new { message = "Producto no encontrado." });

        return Ok(product);
    }

    
    [HttpPost]
    public async Task<ActionResult<Product>> Create([FromBody] Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
            return BadRequest(new { message = "Name es requerido." });

        if (product.Price < 0)
            return BadRequest(new { message = "Price no puede ser negativo." });

        if (product.Stock < 0)
            return BadRequest(new { message = "Stock no puede ser negativo." });

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Product product)
    {
        if (id <= 0) return BadRequest(new { message = "Id inválido." });

        var existing = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (existing is null) return NotFound(new { message = "Producto no encontrado." });

        if (string.IsNullOrWhiteSpace(product.Name))
            return BadRequest(new { message = "Name es requerido." });

        if (product.Price < 0)
            return BadRequest(new { message = "Price no puede ser negativo." });

        if (product.Stock < 0)
            return BadRequest(new { message = "Stock no puede ser negativo." });

        existing.Name = product.Name;
        existing.Description = product.Description;
        existing.Price = product.Price;
        existing.Stock = product.Stock;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (existing is null) return NotFound(new { message = "Producto no encontrado." });

        _db.Products.Remove(existing);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
