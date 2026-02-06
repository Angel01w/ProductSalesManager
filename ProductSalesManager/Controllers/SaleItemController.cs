using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductSalesManager.Domain.Entities;
using ProductSalesManager.Infrastructure.Data;

namespace ProductSalesManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SaleItemsController : ControllerBase
{
    private readonly AppDbContext _db;

    public SaleItemsController(AppDbContext db)
    {
        _db = db;
    }

    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _db.SaleItems
            .AsNoTracking()
            .Include(i => i.Sale)
            .Include(i => i.Product)
            .OrderByDescending(i => i.Id)
            .Select(i => new
            {
                i.Id,
                i.SaleId,
                i.ProductId,
                ProductName = i.Product.Name,
                i.Quantity,
                i.UnitPrice,
                LineTotal = i.Quantity * i.UnitPrice
            })
            .ToListAsync();

        return Ok(items);
    }

    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _db.SaleItems
            .AsNoTracking()
            .Include(i => i.Sale)
            .Include(i => i.Product)
            .Where(i => i.Id == id)
            .Select(i => new
            {
                i.Id,
                i.SaleId,
                i.ProductId,
                ProductName = i.Product.Name,
                i.Quantity,
                i.UnitPrice,
                LineTotal = i.Quantity * i.UnitPrice
            })
            .FirstOrDefaultAsync();

        if (item is null) return NotFound(new { message = "Detalle de venta no encontrado." });

        return Ok(item);
    }

    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaleItem saleItem)
    {
        if (saleItem.SaleId <= 0) return BadRequest(new { message = "SaleId inválido." });
        if (saleItem.ProductId <= 0) return BadRequest(new { message = "ProductId inválido." });
        if (saleItem.Quantity <= 0) return BadRequest(new { message = "Quantity debe ser > 0." });
        if (saleItem.UnitPrice < 0) return BadRequest(new { message = "UnitPrice no puede ser negativo." });

        var saleExists = await _db.Sales.AnyAsync(s => s.Id == saleItem.SaleId);
        if (!saleExists) return NotFound(new { message = "La venta (SaleId) no existe." });

        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == saleItem.ProductId);
        if (product is null) return NotFound(new { message = "El producto (ProductId) no existe." });

        _db.SaleItems.Add(saleItem);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = saleItem.Id }, new { saleItem.Id });
    }

    
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] SaleItem saleItem)
    {
        var existing = await _db.SaleItems.FirstOrDefaultAsync(i => i.Id == id);
        if (existing is null) return NotFound(new { message = "Detalle de venta no encontrado." });

        if (saleItem.SaleId <= 0) return BadRequest(new { message = "SaleId inválido." });
        if (saleItem.ProductId <= 0) return BadRequest(new { message = "ProductId inválido." });
        if (saleItem.Quantity <= 0) return BadRequest(new { message = "Quantity debe ser > 0." });
        if (saleItem.UnitPrice < 0) return BadRequest(new { message = "UnitPrice no puede ser negativo." });

        var saleExists = await _db.Sales.AnyAsync(s => s.Id == saleItem.SaleId);
        if (!saleExists) return NotFound(new { message = "La venta (SaleId) no existe." });

        var productExists = await _db.Products.AnyAsync(p => p.Id == saleItem.ProductId);
        if (!productExists) return NotFound(new { message = "El producto (ProductId) no existe." });

        existing.SaleId = saleItem.SaleId;
        existing.ProductId = saleItem.ProductId;
        existing.Quantity = saleItem.Quantity;
        existing.UnitPrice = saleItem.UnitPrice;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.SaleItems.FirstOrDefaultAsync(i => i.Id == id);
        if (existing is null) return NotFound(new { message = "Detalle de venta no encontrado." });

        _db.SaleItems.Remove(existing);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
