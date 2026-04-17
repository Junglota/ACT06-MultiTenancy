namespace ACT06_MultiTenancy.Domain.Entities
{
    public class Articulo
    {
        public Guid Id { get; set; }

        public string TenantId { get; set; } = default!;

        public string Codigo { get; set; } = default!;
        public string Nombre { get; set; } = default!;
        public string? Descripcion { get; set; }
        public int Stock { get; set; }

        public Guid? TipoEquipoId { get; set; }
        public TipoEquipo? TipoEquipo { get; set; }

        public Guid? SedeId { get; set; }
        public Sede? Sede { get; set; }

        public string EstadoOperativo { get; set; } = "Disponible";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
