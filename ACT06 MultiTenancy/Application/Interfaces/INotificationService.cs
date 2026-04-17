using ACT06_MultiTenancy.Api.Models;

namespace ACT06_MultiTenancy.Application.Interfaces
{
    public interface INotificationService
    {
        Task CreateAsync(Notification notification);
        Task CreateLoanCreatedAsync(string tenantId, Guid userId, Loan loan, string articleName);
        Task CreateLoanReturnedAsync(string tenantId, Guid userId, Loan loan, string articleName);
    }
}
