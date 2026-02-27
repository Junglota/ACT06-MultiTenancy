namespace ACT06_MultiTenancy.Application.Interfaces
{
    public interface ITenantProvider
    {
        string? TenantId { get; }
    }
}
