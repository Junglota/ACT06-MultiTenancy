using ACT06_MultiTenancy.Api.Models;
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
        public DbSet<Loan> Loans { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Note>().HasQueryFilter(x => x.TenantId == CurrentTenantId);

            modelBuilder.Entity<Articulo>().HasQueryFilter(x => x.TenantId == CurrentTenantId);

            modelBuilder.Entity<Articulo>()
                .HasIndex(x => new { x.TenantId, x.Codigo })
                .IsUnique();

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Loan>(entity =>
            {
                entity.ToTable("Loans");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Status)
                      .HasMaxLength(30)
                      .HasDefaultValue("Active");

                entity.HasIndex(x => x.TenantId);
                entity.HasIndex(x => new { x.TenantId, x.ArticleId, x.Status });

                entity.HasOne(x => x.Article)
                      .WithMany()
                      .HasForeignKey(x => x.ArticleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

        }
    }
}
