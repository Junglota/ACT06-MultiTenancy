namespace ACT06_MultiTenancy.Domain.Entities
{
    public class Note
    {
        public Guid Id { get; set; }
        public string TenantId { get; set; } = default!;
        public string Title { get; set; } = default!;
    }
}
