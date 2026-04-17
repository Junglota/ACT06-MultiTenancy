using ACT06_MultiTenancy.Api.Models;
using ACT06_MultiTenancy.Application.Interfaces;
using ACT06_MultiTenancy.Infrastructure.Data;

namespace ACT06_MultiTenancy.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;

        public NotificationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(Notification notification)
        {
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
        }

        public async Task CreateLoanCreatedAsync(string tenantId, Guid userId, Loan loan, string articleName)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                LoanId = loan.Id,
                ArticleId = loan.ArticleId,
                Type = "LoanCreated",
                Title = "Préstamo registrado",
                Message = $"Tu préstamo del artículo '{articleName}' fue registrado correctamente.",
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
        }

        public async Task CreateLoanReturnedAsync(string tenantId, Guid userId, Loan loan, string articleName)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                UserId = userId,
                LoanId = loan.Id,
                ArticleId = loan.ArticleId,
                Type = "LoanReturned",
                Title = "Préstamo devuelto",
                Message = $"La devolución del artículo '{articleName}' fue registrada correctamente.",
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
        }
    }
}
