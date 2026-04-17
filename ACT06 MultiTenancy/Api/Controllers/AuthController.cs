using ACT06_MultiTenancy.Api.DTos;
using ACT06_MultiTenancy.Api.Models;
using ACT06_MultiTenancy.Application.Interfaces;
using ACT06_MultiTenancy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _users;
    private readonly ITokenGenerator _tokens;
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenant;
    private readonly Serilog.ILogger _log;

    public AuthController(IUserRepository users, ITokenGenerator tokens, AppDbContext db, ITenantProvider tenant)
    {
        _users = users;
        _tokens = tokens;
        _db = db;
        _tenant = tenant;
        _log = Log.ForContext<AuthController>();
    }

    [HttpPost("/Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await _users.GetByUsernameAsync(req.Username, ct);
        if (user is null) return Unauthorized("Credenciales inválidas");

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Credenciales inválidas");

        var token = _tokens.Generate(user);
        return Ok(new { token });
    }

    [Authorize]
    [HttpPost("/CambioDeClave")]
    public async Task<IActionResult> CambioDeClave([FromBody] ChangePasswordRequest req, CancellationToken ct)
    {
        var tokenUsername = User.FindFirst("username")?.Value;
        if (!string.Equals(tokenUsername, req.Username, StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var user = await _users.GetByUsernameAsync(req.Username, ct);
        if (user is null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
            return BadRequest("Contraseña actual incorrecta");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        await _users.UpdateAsync(user, ct);

        return Ok("Contraseña actualizada");
    }

    [HttpPost("/OlvideMiClave")]
    public IActionResult OlvideMiClave([FromBody] ForgotPasswordRequest req)
    {
        var code = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        _log.Information("Solicitud OlvideMiClave para {User}. Codigo={Code}", req.UsernameOrEmail, code);

        return Ok("Si el usuario existe, se enviará un código de recuperación (simulado).");
    }

    [Authorize]
    [HttpGet("/api/dashboard")]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var tenantId = _tenant.TenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
            return Unauthorized(new { message = "Tenant no encontrado." });

        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            User.FindFirst("sub")?.Value ??
            User.FindFirst("userId")?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new { message = "Usuario no autenticado." });

        var user = await _db.Users
            .FirstOrDefaultAsync(x => x.Id == userId && x.TenantId == tenantId, ct);

        if (user == null)
            return Unauthorized(new { message = "Usuario no válido para este tenant." });

        var now = DateTime.UtcNow;
        var sevenDaysFromNow = now.AddDays(7);

        var activeLoans = await _db.Loans
            .Include(x => x.Article)
            .Where(x => x.TenantId == tenantId && x.Status == "Active")
            .OrderBy(x => x.DueDate)
            .ToListAsync(ct);

        var unreadCount = await _db.Notifications
            .CountAsync(x => x.TenantId == tenantId &&
                             x.UserId == user.Id &&
                             !x.IsRead, ct);

        var recentNotifications = await _db.Notifications
            .Where(x => x.TenantId == tenantId && x.UserId == user.Id)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(5)
            .ToListAsync(ct);

        var weeklyReturns = activeLoans.Count(x =>
            x.DueDate >= now && x.DueDate <= sevenDaysFromNow);

        var activeLoansTable = activeLoans
            .Take(10)
            .Select(x => new DashboardLoanRowDto
            {
                Id = x.Id,
                ArticleName = x.Article != null ? x.Article.Nombre : "Artículo",
                LoanDate = x.LoanDate,
                DueDate = x.DueDate,
                Status = GetLoanVisualStatus(x.DueDate, now)
            })
            .ToList();

        var upcomingReturns = activeLoans
            .Take(3)
            .Select(x => new DashboardUpcomingReturnDto
            {
                Id = x.Id,
                ArticleName = x.Article != null ? x.Article.Nombre : "Artículo",
                DueDate = x.DueDate,
                Label = GetUpcomingLabel(x.DueDate, now)
            })
            .ToList();

        var response = new DashboardResponseDto
        {
            User = new DashboardUserDto
            {
                Username = user.Username,
                Role = user.Role
            },
            Notifications = new DashboardNotificationsDto
            {
                UnreadCount = unreadCount,
                Recent = recentNotifications.Select(x => new DashboardNotificationDto
                {
                    Id = x.Id,
                    Type = x.Type,
                    Title = x.Title,
                    Message = x.Message,
                    IsRead = x.IsRead,
                    CreatedAtUtc = x.CreatedAtUtc
                }).ToList()
            },
            Stats = new DashboardStatsDto
            {
                ActiveLoans = activeLoans.Count,
                WeeklyReturns = weeklyReturns
            },
            ActiveLoansTable = activeLoansTable,
            UpcomingReturns = upcomingReturns
        };

        return Ok(response);
    }

    private static string GetLoanVisualStatus(DateTime dueDate, DateTime now)
    {
        if (dueDate < now)
            return "Atrasado";

        if (dueDate <= now.AddDays(1))
            return "Por vencer";

        return "En tiempo";
    }

    private static string GetUpcomingLabel(DateTime dueDate, DateTime now)
    {
        if (dueDate < now)
            return "Urgente";

        if (dueDate.Date == now.Date)
            return "Hoy";

        return "Próximo";
    }
}