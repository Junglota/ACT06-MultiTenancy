using ACT06_MultiTenancy.Application.Interfaces;
using ACT06_MultiTenancy.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ACT06_MultiTenancy.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        private readonly ITenantProvider _tenantProvider;

        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
            : base(options)
        {
            _tenantProvider = tenantProvider;
        }

        public string? CurrentTenantId => _tenantProvider.TenantId;

        public DbSet<User> Users => Set<User>();
        public DbSet<Note> Notes => Set<Note>();
        public DbSet<Articulo> Articulos => Set<Articulo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Note>().HasQueryFilter(x => x.TenantId == CurrentTenantId);

            modelBuilder.Entity<Articulo>().HasQueryFilter(x => x.TenantId == CurrentTenantId);

            modelBuilder.Entity<Articulo>()
                .HasIndex(x => new { x.TenantId, x.Codigo })
                .IsUnique();

            base.OnModelCreating(modelBuilder);

        }
    }
}
