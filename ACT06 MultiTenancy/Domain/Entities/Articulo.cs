namespace ACT06_MultiTenancy.Domain.Entities
{
    public class Articulo
    {
        public Guid Id { get; set; }

        public string TenantId { get; set; } = default!;

        public string Codigo { get; set; } = default!;
        public string Nombre { get; set; } = default!;
        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
