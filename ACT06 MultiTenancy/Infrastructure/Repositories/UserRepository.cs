using ACT06_MultiTenancy.Application.Interfaces;
using ACT06_MultiTenancy.Domain.Entities;
using ACT06_MultiTenancy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ACT06_MultiTenancy.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;
        public UserRepository(AppDbContext db) => _db = db;

        public Task<User?> GetByUsernameAsync(string username, CancellationToken ct) =>
            _db.Users.FirstOrDefaultAsync(x => x.Username == username, ct);

        public async Task UpdateAsync(User user, CancellationToken ct)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync(ct);
        }
    }
}
