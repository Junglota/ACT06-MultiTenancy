using ACT06_MultiTenancy.Api.DTos;
using ACT06_MultiTenancy.Api.Models;
using ACT06_MultiTenancy.Application.Interfaces;
using ACT06_MultiTenancy.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ACT06_MultyTenancy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ITenantProvider _tenant;

        public LoansController(AppDbContext context, ITenantProvider tenant)
        {
            _db = context;
            _tenant = tenant;

        }

        [HttpGet]
        public async Task<IActionResult> GetLoans()
        {
            var tenantId = _tenant.TenantId;

            var loans = await _db.Loans
                .Include(x => x.Article)
                .Where(x => x.TenantId == tenantId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(loans);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLoanById(int id)
        {
            var tenantId = _tenant.TenantId;

            var loan = await _db.Loans
                .Include(x => x.Article)
                .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

            if (loan == null)
                return NotFound(new { message = "Préstamo no encontrado." });

            return Ok(loan);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLoan([FromBody] CreateLoanDto dto)
        {
            var tenantId = _tenant.TenantId;

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var article = await _db.Articulos
                .FirstOrDefaultAsync(x => x.Id == dto.ArticleId && x.TenantId == tenantId);

            if (article == null)
                return NotFound(new { message = "Artículo no encontrado para este tenant." });

            var activeLoanExists = await _db.Loans.AnyAsync(x =>
                x.TenantId == tenantId &&
                x.ArticleId == dto.ArticleId &&
                x.Status == "Active");

            if (activeLoanExists)
                return BadRequest(new { message = "El artículo ya tiene un préstamo activo." });

            var loan = new Loan
            {
                TenantId = tenantId,
                ArticleId = dto.ArticleId,
                BorrowerName = dto.BorrowerName,
                BorrowerEmail = dto.BorrowerEmail,
                LoanDate = DateTime.UtcNow,
                DueDate = dto.DueDate,
                Notes = dto.Notes,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            _db.Loans.Add(loan);

            // opcional: marcar artículo como no disponible
            // article.IsAvailable = false;

            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLoanById), new { id = loan.Id }, loan);
        }

        [HttpPut("{id}/return")]
        public async Task<IActionResult> ReturnLoan(int id, [FromBody] ReturnLoanDto? dto)
        {
            var tenantId = _tenant.TenantId;

            var loan = await _db.Loans
                .Include(x => x.Article)
                .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

            if (loan == null)
                return NotFound(new { message = "Préstamo no encontrado." });

            if (loan.Status == "Returned")
                return BadRequest(new { message = "El préstamo ya fue devuelto." });

            loan.Status = "Returned";
            loan.ReturnedAt = DateTime.UtcNow;
            loan.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto?.Notes))
                loan.Notes = dto.Notes;

            // opcional: marcar artículo como disponible
            // if (loan.Article != null)
            //     loan.Article.IsAvailable = true;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Préstamo devuelto correctamente.", loan });
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveLoans()
        {
            var tenantId = _tenant.TenantId;

            var loans = await _db.Loans
                .Include(x => x.Article)
                .Where(x => x.TenantId == tenantId && x.Status == "Active")
                .OrderBy(x => x.DueDate)
                .ToListAsync();

            return Ok(loans);
        }
    }
}