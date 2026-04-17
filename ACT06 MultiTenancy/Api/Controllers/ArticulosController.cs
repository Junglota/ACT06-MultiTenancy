using ACT06_MultiTenancy.Api.Models;
using ACT06_MultiTenancy.Application.Interfaces;
using ACT06_MultiTenancy.Domain.Entities;
using ACT06_MultiTenancy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using static ACT06_MultiTenancy.Api.DTos.EquiposDto;

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
                x.Codigo,
                x.Nombre,
                x.Descripcion,
                x.Stock
            ))
            .ToListAsync(ct);

        return Ok(items);
    }

    // GET: api/articulos/codigo/{codigo}
    [HttpGet("codigo/{codigo}")]
    public async Task<IActionResult> GetByCodigo(Guid id, CancellationToken ct)
    {
        var item = await _db.Articulos
            .Where(x => x.Id == id)
            .Select(x => new ArticuloResponse(
                x.Codigo,
                x.Nombre,
                x.Descripcion,
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

        return CreatedAtAction(nameof(GetByCodigo), new { Codigo = entity.Codigo }, entity);
    }

    // PUT: api/articulos/{codigo}
    [HttpPut("codigo/{codigo}")]
    public async Task<IActionResult> Update(string codigo, [FromBody] ArticuloUpdateRequest req, CancellationToken ct)
    {
        var item = await _db.Articulos.FirstOrDefaultAsync(x => x.Codigo == codigo, ct);
        if (item is null) return NotFound();

        item.Codigo = req.Codigo.Trim();
        item.Nombre = req.Nombre.Trim();
        item.Descripcion = req.Descripcion?.Trim();
        item.UpdatedAtUtc = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            _log.Warning(ex, "Error actualizando artículo. Codigo={Codigo}", codigo);
            return Conflict("Conflicto actualizando el artículo (código duplicado en este tenant).");
        }

        return Ok(item);
    }

    // DELETE: api/articulos/{codigo}
    [HttpDelete("codigo/{codigo}")]
    public async Task<IActionResult> Delete(string codigo, CancellationToken ct)
    {
        var item = await _db.Articulos.FirstOrDefaultAsync(x => x.Codigo == codigo, ct);
        if (item is null) return NotFound();

        _db.Articulos.Remove(item);
        await _db.SaveChangesAsync(ct);

        return NoContent();
    }

    // POST: api/articulos/codigo/{codigo}/entrada
    [HttpPost("codigo/{codigo}/entrada")]
    public async Task<IActionResult> Entrada(string codigo, [FromBody] MovimientoInventarioRequest req, CancellationToken ct)
    {
        if (req.Cantidad <= 0) return BadRequest("Cantidad debe ser mayor que 0.");

        var item = await _db.Articulos.FirstOrDefaultAsync(x => x.Codigo == codigo, ct);
        if (item is null) return NotFound();

        item.Stock += req.Cantidad;
        item.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _log.Information("Entrada inventario. Articulo={codigo} Cantidad={Cantidad} NuevoStock={Stock} Nota={Nota}",
            codigo, req.Cantidad, item.Stock, req.Nota);

        return Ok(new {item.Codigo, item.Nombre, item.Stock });
    }

    // POST: api/articulos/codigo/{codigo}/salida
    [HttpPost("codigo/{codigo}/salida")]
    public async Task<IActionResult> Salida(string codigo, [FromBody] MovimientoInventarioRequest req, CancellationToken ct)
    {
        if (req.Cantidad <= 0) return BadRequest("Cantidad debe ser mayor que 0.");

        var item = await _db.Articulos.FirstOrDefaultAsync(x => x.Codigo == codigo, ct);
        if (item is null) return NotFound();

        if (item.Stock - req.Cantidad < 0)
            return BadRequest($"Stock insuficiente. Stock actual={item.Stock}.");

        item.Stock -= req.Cantidad;
        item.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        _log.Information("Salida inventario. Articulo={codigo} Cantidad={Cantidad} NuevoStock={Stock} Nota={Nota}",
            codigo, req.Cantidad, item.Stock, req.Nota);

        return Ok(new {item.Codigo, item.Nombre, item.Stock });
    }


    [HttpGet("catalogo")]
    public async Task<IActionResult> GetEquipos(
            [FromQuery] Guid? tipoId,
            [FromQuery] Guid? sedeId,
            [FromQuery] string? estado,
            CancellationToken ct)
    {
        var tenantId = _tenant.TenantId;

        if (string.IsNullOrWhiteSpace(tenantId))
            return Unauthorized(new { message = "Tenant no encontrado." });

        var tipos = await _db.TiposEquipo
            .OrderBy(x => x.Nombre)
            .Select(x => new TipoEquipoFilterDto
            {
                Id = x.Id,
                Nombre = x.Nombre
            })
            .ToListAsync(ct);

        var sedes = await _db.Sedes
            .OrderBy(x => x.Nombre)
            .Select(x => new SedeFilterDto
            {
                Id = x.Id,
                Nombre = x.Nombre
            })
            .ToListAsync(ct);

        var articulos = await _db.Articulos
            .Include(x => x.TipoEquipo)
            .Include(x => x.Sede)
            .OrderBy(x => x.Nombre)
            .ToListAsync(ct);

        var articleIds = articulos.Select(x => x.Id).ToList();

        var activeLoansGrouped = await _db.Loans
            .Where(x => x.TenantId == tenantId &&
                        x.Status == "Active" &&
                        articleIds.Contains(x.ArticleId))
            .GroupBy(x => x.ArticleId)
            .Select(g => new
            {
                ArticleId = g.Key,
                Count = g.Count()
            })
            .ToListAsync(ct);

        var activeLoansMap = activeLoansGrouped.ToDictionary(x => x.ArticleId, x => x.Count);

        var items = articulos.Select(a =>
        {
            activeLoansMap.TryGetValue(a.Id, out var prestamosActivos);

            var estadoCalculado = GetEstadoArticulo(
                a.EstadoOperativo,
                a.Stock,
                prestamosActivos
            );

            return new EquipoCardDto
            {
                Id = a.Id,
                Codigo = a.Codigo,
                Nombre = a.Nombre,
                Descripcion = a.Descripcion,
                TipoEquipo = a.TipoEquipo?.Nombre ?? "Sin tipo",
                Sede = a.Sede?.Nombre,
                Stock = a.Stock,
                PrestamosActivos = prestamosActivos,
                Estado = estadoCalculado
            };
        });

        if (tipoId.HasValue)
        {
            items = items.Where(x =>
                articulos.Any(a => a.Id == x.Id && a.TipoEquipoId == tipoId.Value));
        }

        if (sedeId.HasValue)
        {
            items = items.Where(x =>
                articulos.Any(a => a.Id == x.Id && a.SedeId == sedeId.Value));
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            items = items.Where(x => x.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase));
        }

        var orderedItems = items
            .OrderBy(x => GetEstadoOrder(x.Estado))
            .ThenBy(x => x.Nombre)
            .ToList();

        var response = new EquiposResponseDto
        {
            Filters = new EquiposFiltersDto
            {
                Tipos = tipos,
                Sedes = sedes,
                Estados = new List<string>
                    {
                        "Disponible",
                        "Prestado",
                        "Mantenimiento"
                    }
            },
            Items = orderedItems
        };

        return Ok(response);
    }

    private static string GetEstadoArticulo(string? estadoOperativo, int stock, int prestamosActivos)
    {
        if (string.Equals(estadoOperativo, "Mantenimiento", StringComparison.OrdinalIgnoreCase))
            return "Mantenimiento";

        return stock > prestamosActivos ? "Disponible" : "Prestado";
    }

    private static int GetEstadoOrder(string estado)
    {
        return estado switch
        {
            "Disponible" => 0,
            "Prestado" => 1,
            "Mantenimiento" => 2,
            _ => 3
        };
    }


}