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
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<TipoEquipo> TiposEquipo => Set<TipoEquipo>();
        public DbSet<Sede> Sedes => Set<Sede>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Note>().HasQueryFilter(x => x.TenantId == CurrentTenantId);

            // Configuración de Articulo
            modelBuilder.Entity<Articulo>().HasQueryFilter(x => x.TenantId == CurrentTenantId);

            modelBuilder.Entity<Articulo>()
                .HasIndex(x => new { x.TenantId, x.Codigo })
                .IsUnique();

            base.OnModelCreating(modelBuilder);

            // Configuración de Loan
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

            // Configuración de Notification
            modelBuilder.Entity<Notification>().HasQueryFilter(x => x.TenantId == CurrentTenantId);
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Type)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(x => x.Title)
                      .HasMaxLength(200)
                      .IsRequired();

                entity.Property(x => x.Message)
                      .HasMaxLength(1000)
                      .IsRequired();

                entity.Property(x => x.IsRead)
                      .HasDefaultValue(false);

                entity.HasIndex(x => x.TenantId);
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => new { x.TenantId, x.UserId, x.IsRead });
                entity.HasIndex(x => x.CreatedAtUtc);
            });

            modelBuilder.Entity<TipoEquipo>().HasQueryFilter(x => x.TenantId == CurrentTenantId);
            modelBuilder.Entity<Sede>().HasQueryFilter(x => x.TenantId == CurrentTenantId);

            modelBuilder.Entity<TipoEquipo>(entity =>
            {
                entity.ToTable("TiposEquipo");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Nombre)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasIndex(x => new { x.TenantId, x.Nombre })
                      .IsUnique();
            });

            modelBuilder.Entity<Sede>(entity =>
            {
                entity.ToTable("Sedes");

                entity.HasKey(x => x.Id);

                entity.Property(x => x.Nombre)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasIndex(x => new { x.TenantId, x.Nombre })
                      .IsUnique();
            });

            modelBuilder.Entity<Articulo>(entity =>
            {
                entity.HasOne(x => x.TipoEquipo)
                      .WithMany(x => x.Articulos)
                      .HasForeignKey(x => x.TipoEquipoId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Sede)
                      .WithMany(x => x.Articulos)
                      .HasForeignKey(x => x.SedeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(x => x.EstadoOperativo)
                      .HasMaxLength(30)
                      .HasDefaultValue("Disponible");
            });

        }
    }
}
