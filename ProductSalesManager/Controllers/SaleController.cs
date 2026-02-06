using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductSalesManager.Domain.Entities;
using ProductSalesManager.Infrastructure.Data;

namespace ProductSalesManager.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly AppDbContext _db;

    public SalesController(AppDbContext db)
    {
        _db = db;
    }

    
    public class SaleItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CreateSaleRequest
    {
        public int CustomerId { get; set; }
        public DateTime? Date { get; set; }
        public List<SaleItemRequest> Items { get; set; } = new();
    }

    public class UpdateSaleRequest
    {
        public int CustomerId { get; set; }
        public DateTime? Date { get; set; }
        public List<SaleItemRequest> Items { get; set; } = new();
    }

  
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sales = await _db.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .OrderByDescending(s => s.Id)
            .Select(s => new
            {
                s.Id,
                s.Date,
                Customer = new { s.Customer.Id, s.Customer.Name, s.Customer.Email, s.Customer.Phone },
                s.Total,
                Items = s.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    ProductName = i.Product.Name,
                    i.Quantity,
                    i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice
                })
            })
            .ToListAsync();

        return Ok(sales);
    }

    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var sale = await _db.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.Items)
                .ThenInclude(i => i.Product)
            .Where(s => s.Id == id)
            .Select(s => new
            {
                s.Id,
                s.Date,
                Customer = new { s.Customer.Id, s.Customer.Name, s.Customer.Email, s.Customer.Phone },
                s.Total,
                Items = s.Items.Select(i => new
                {
                    i.Id,
                    i.ProductId,
                    ProductName = i.Product.Name,
                    i.Quantity,
                    i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice
                })
            })
            .FirstOrDefaultAsync();

        if (sale is null) return NotFound(new { message = "Venta no encontrada." });

        return Ok(sale);
    }

    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest req)
    {
        if (req.CustomerId <= 0) return BadRequest(new { message = "CustomerId inválido." });
        if (req.Items is null || req.Items.Count == 0) return BadRequest(new { message = "Debe incluir al menos 1 producto." });
        if (req.Items.Any(i => i.ProductId <= 0 || i.Quantity <= 0))
            return BadRequest(new { message = "Cada item debe tener ProductId > 0 y Quantity > 0." });

        var customerExists = await _db.Customers.AnyAsync(c => c.Id == req.CustomerId);
        if (!customerExists) return NotFound(new { message = "Cliente no encontrado." });

        
        var grouped = req.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .ToList();

        var productIds = grouped.Select(x => x.ProductId).ToList();

        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        if (products.Count != productIds.Count)
            return BadRequest(new { message = "Uno o más ProductId no existen." });

        
        foreach (var g in grouped)
        {
            var p = products.First(x => x.Id == g.ProductId);
            if (p.Stock < g.Quantity)
                return BadRequest(new { message = $"Stock insuficiente para el producto '{p.Name}'. Disponible: {p.Stock}, solicitado: {g.Quantity}." });
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        
        var sale = new Sale
        {
            CustomerId = req.CustomerId,
            Date = req.Date ?? DateTime.UtcNow,
            Total = 0m,
            Items = new List<SaleItem>()
        };

        decimal total = 0m;

        foreach (var g in grouped)
        {
            var p = products.First(x => x.Id == g.ProductId);

            
            var unitPrice = p.Price;

            sale.Items.Add(new SaleItem
            {
                ProductId = p.Id,
                Quantity = g.Quantity,
                UnitPrice = unitPrice
            });

            total += g.Quantity * unitPrice;

            
            p.Stock -= g.Quantity;
        }

        sale.Total = total;

        _db.Sales.Add(sale);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return CreatedAtAction(nameof(GetById), new { id = sale.Id }, new { sale.Id });
    }

    
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSaleRequest req)
    {
        if (id <= 0) return BadRequest(new { message = "Id inválido." });
        if (req.CustomerId <= 0) return BadRequest(new { message = "CustomerId inválido." });
        if (req.Items is null || req.Items.Count == 0) return BadRequest(new { message = "Debe incluir al menos 1 producto." });
        if (req.Items.Any(i => i.ProductId <= 0 || i.Quantity <= 0))
            return BadRequest(new { message = "Cada item debe tener ProductId > 0 y Quantity > 0." });

        var sale = await _db.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale is null) return NotFound(new { message = "Venta no encontrada." });

        var customerExists = await _db.Customers.AnyAsync(c => c.Id == req.CustomerId);
        if (!customerExists) return NotFound(new { message = "Cliente no encontrado." });

        
        var oldItems = sale.Items.ToList();
        var oldProductIds = oldItems.Select(i => i.ProductId).Distinct().ToList();

        var oldProducts = await _db.Products
            .Where(p => oldProductIds.Contains(p.Id))
            .ToListAsync();

        foreach (var item in oldItems)
        {
            var p = oldProducts.First(x => x.Id == item.ProductId);
            p.Stock += item.Quantity;
        }

        
        var grouped = req.Items
            .GroupBy(i => i.ProductId)
            .Select(g => new { ProductId = g.Key, Quantity = g.Sum(x => x.Quantity) })
            .ToList();

        var newProductIds = grouped.Select(x => x.ProductId).ToList();

        var products = await _db.Products
            .Where(p => newProductIds.Contains(p.Id))
            .ToListAsync();

        if (products.Count != newProductIds.Count)
            return BadRequest(new { message = "Uno o más ProductId no existen." });


        foreach (var g in grouped)
        {
            var p = products.First(x => x.Id == g.ProductId);
            if (p.Stock < g.Quantity)
                return BadRequest(new { message = $"Stock insuficiente para el producto '{p.Name}'. Disponible: {p.Stock}, solicitado: {g.Quantity}." });
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

     
        sale.CustomerId = req.CustomerId;
        sale.Date = req.Date ?? sale.Date;

        
        _db.SaleItems.RemoveRange(oldItems);
        sale.Items.Clear();

        
        decimal total = 0m;

        foreach (var g in grouped)
        {
            var p = products.First(x => x.Id == g.ProductId);
            var unitPrice = p.Price;

            sale.Items.Add(new SaleItem
            {
                ProductId = p.Id,
                Quantity = g.Quantity,
                UnitPrice = unitPrice
            });

            total += g.Quantity * unitPrice;
            p.Stock -= g.Quantity;
        }

        sale.Total = total;

        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return NoContent();
    }

    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var sale = await _db.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale is null) return NotFound(new { message = "Venta no encontrada." });

        var productIds = sale.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        await using var tx = await _db.Database.BeginTransactionAsync();

        // devolver stock
        foreach (var item in sale.Items)
        {
            var p = products.First(x => x.Id == item.ProductId);
            p.Stock += item.Quantity;
        }

        _db.Sales.Remove(sale); 
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return NoContent();
    }
}
