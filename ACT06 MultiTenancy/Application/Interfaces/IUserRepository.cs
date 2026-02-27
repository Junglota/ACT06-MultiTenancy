using ACT06_MultiTenancy.Domain.Entities;

namespace ACT06_MultiTenancy.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username, CancellationToken ct);
        Task UpdateAsync(User user, CancellationToken ct);
    }
}
