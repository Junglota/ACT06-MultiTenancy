using System.Security.Claims;
using ACT06_MultiTenancy.Application.Interfaces;
using ACT06_MultiTenancy.Domain.Entities;
using ACT06_MultiTenancy.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ACT06_MultyTenancy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ITenantProvider _tenant;

        public NotificationsController(AppDbContext db, ITenantProvider tenant)
        {
            _db = db;
            _tenant = tenant;
        }

        private Guid? GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("sub")?.Value ??
                User.FindFirst("userId")?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;

            return null;
        }

        private async Task<User?> GetCurrentUserAsync(string tenantId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return null;

            return await _db.Users
                .FirstOrDefaultAsync(x => x.Id == userId.Value && x.TenantId == tenantId);
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var tenantId = _tenant.TenantId;

            if (string.IsNullOrWhiteSpace(tenantId))
                return Unauthorized(new { message = "Tenant no encontrado." });

            var user = await GetCurrentUserAsync(tenantId);
            if (user == null)
                return Unauthorized(new { message = "Usuario no autenticado o no válido para este tenant." });

            var notifications = await _db.Notifications
                .Where(x => x.TenantId == tenantId && x.UserId == user.Id)
                .OrderByDescending(x => x.CreatedAtUtc)
                .ToListAsync();

            return Ok(notifications);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var tenantId = _tenant.TenantId;

            if (string.IsNullOrWhiteSpace(tenantId))
                return Unauthorized(new { message = "Tenant no encontrado." });

            var user = await GetCurrentUserAsync(tenantId);
            if (user == null)
                return Unauthorized(new { message = "Usuario no autenticado o no válido para este tenant." });

            var count = await _db.Notifications
                .CountAsync(x => x.TenantId == tenantId &&
                                 x.UserId == user.Id &&
                                 !x.IsRead);

            return Ok(new { count });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetNotificationById(Guid id)
        {
            var tenantId = _tenant.TenantId;

            if (string.IsNullOrWhiteSpace(tenantId))
                return Unauthorized(new { message = "Tenant no encontrado." });

            var user = await GetCurrentUserAsync(tenantId);
            if (user == null)
                return Unauthorized(new { message = "Usuario no autenticado o no válido para este tenant." });

            var notification = await _db.Notifications
                .FirstOrDefaultAsync(x => x.Id == id &&
                                          x.TenantId == tenantId &&
                                          x.UserId == user.Id);

            if (notification == null)
                return NotFound(new { message = "Notificación no encontrada." });

            return Ok(notification);
        }

        [HttpPut("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            var tenantId = _tenant.TenantId;

            if (string.IsNullOrWhiteSpace(tenantId))
                return Unauthorized(new { message = "Tenant no encontrado." });

            var user = await GetCurrentUserAsync(tenantId);
            if (user == null)
                return Unauthorized(new { message = "Usuario no autenticado o no válido para este tenant." });

            var notification = await _db.Notifications
                .FirstOrDefaultAsync(x => x.Id == id &&
                                          x.TenantId == tenantId &&
                                          x.UserId == user.Id);

            if (notification == null)
                return NotFound(new { message = "Notificación no encontrada." });

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAtUtc = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            return Ok(new { message = "Notificación marcada como leída." });
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var tenantId = _tenant.TenantId;

            if (string.IsNullOrWhiteSpace(tenantId))
                return Unauthorized(new { message = "Tenant no encontrado." });

            var user = await GetCurrentUserAsync(tenantId);
            if (user == null)
                return Unauthorized(new { message = "Usuario no autenticado o no válido para este tenant." });

            var notifications = await _db.Notifications
                .Where(x => x.TenantId == tenantId &&
                            x.UserId == user.Id &&
                            !x.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAtUtc = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Todas las notificaciones fueron marcadas como leídas.",
                updated = notifications.Count
            });
        }
    }
}