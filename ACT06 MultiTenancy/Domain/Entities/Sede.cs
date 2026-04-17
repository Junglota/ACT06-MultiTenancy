namespace ACT06_MultiTenancy.Domain.Entities
{
    public class Sede
    {
        public Guid Id { get; set; }
        public string TenantId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        public ICollection<Articulo> Articulos { get; set; } = new List<Articulo>();
    }
}
