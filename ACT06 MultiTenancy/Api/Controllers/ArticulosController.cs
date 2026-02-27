using ACT06_MultiTenancy.Api.Models;
using ACT06_MultiTenancy.Application.Interfaces;
using ACT06_MultiTenancy.Domain.Entities;
using ACT06_MultiTenancy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ArticulosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenant;
    private readonly Serilog.ILogger _log;

    public ArticulosController(AppDbContext db, ITenantProvider tenant)
    {
        _db = db;
        _tenant = tenant;
        _log = Log.ForContext<ArticulosController>();
    }

    // GET: api/articulos
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await _db.Articulos
            .OrderBy(x => x.Nombre)
            .Select(x => new ArticuloResponse(
                x.Id,
                x.Codigo,
                x.Nombre,
                x.Descripcion,
                x.Precio,
                x.Stock
            ))
            .ToListAsync(ct);

        return Ok(items);
    }

    // GET: api/articulos/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var item = await _db.Articulos
            .Where(x => x.Id == id)
            .Select(x => new ArticuloResponse(
                x.Id,
                x.Codigo,
                x.Nombre,
                x.Descripcion,
                x.Precio,
                x.Stock
            ))
            .FirstOrDefaultAsync(ct);

        if (item is null) return NotFound();
        return Ok(item);
    }

    // POST: api/articulos
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ArticuloCreateRequest req, CancellationToken ct)
    {
        if (req.StockInicial < 0) return BadRequest("StockInicial no puede ser negativo.");

        var tenantId = _tenant.TenantId;
        if (string.IsNullOrWhiteSpace(tenantId)) return Unauthorized("TenantId no presente en el token.");

        var entity = new Articulo
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Codigo = req.Codigo.Trim(),
            Nombre = req.Nombre.Trim(),
            Descripcion = req.Descripcion?.Trim(),
            Precio = req.Precio,
            Stock = req.StockInicial,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _db.Articulos.Add(entity);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            _log.Warning(ex, "Error creando artículo. Tenant={TenantId} Codigo={Codigo}", tenantId, entity.Codigo);
            return Conflict("Ya existe un artículo con ese código en este tenant.");
        }

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    // PUT: api/articulos/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] ArticuloUpdateRequest req, CancellationToken ct)
    {
        var item = await _db.Articulos.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null) return NotFound();

        item.Codigo = req.Codigo.Trim();
        item.Nombre = req.Nombre.Trim();
        item.Descripcion = req.Descripcion?.Trim();
        item.Precio = req.Precio;
        item.UpdatedAtUtc = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            _log.Warning(ex, "Error actualizando artículo. Id={Id}", id);
            return Conflict("Conflicto actualizando el artículo (código duplicado en este tenant).");
        }

        return Ok(item);
    }

    // DELETE: api/articulos/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var item = await _db.Articulos.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null) return NotFound();

        _db.Articulos.Remove(item);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    // POST: api/articulos/{id}/entrada
    [HttpPost("{id:guid}/entrada")]
    public async Task<IActionResult> Entrada(Guid id, [FromBody] MovimientoInventarioRequest req, CancellationToken ct)
    {
        if (req.Cantidad <= 0) return BadRequest("Cantidad debe ser mayor que 0.");

        var item = await _db.Articulos.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null) return NotFound();

        item.Stock += req.Cantidad;
        item.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _log.Information("Entrada inventario. Articulo={Id} Cantidad={Cantidad} NuevoStock={Stock} Nota={Nota}",
            id, req.Cantidad, item.Stock, req.Nota);

        return Ok(new { item.Id, item.Codigo, item.Nombre, item.Stock });
    }

    // POST: api/articulos/{id}/salida
    [HttpPost("{id:guid}/salida")]
    public async Task<IActionResult> Salida(Guid id, [FromBody] MovimientoInventarioRequest req, CancellationToken ct)
    {
        if (req.Cantidad <= 0) return BadRequest("Cantidad debe ser mayor que 0.");

        var item = await _db.Articulos.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item is null) return NotFound();

        if (item.Stock - req.Cantidad < 0)
            return BadRequest($"Stock insuficiente. Stock actual={item.Stock}.");

        item.Stock -= req.Cantidad;
        item.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _log.Information("Salida inventario. Articulo={Id} Cantidad={Cantidad} NuevoStock={Stock} Nota={Nota}",
            id, req.Cantidad, item.Stock, req.Nota);

        return Ok(new { item.Id, item.Codigo, item.Nombre, item.Stock });
    }
}