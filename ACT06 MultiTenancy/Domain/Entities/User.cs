namespace ACT06_MultiTenancy.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string TenantId { get; set; } = default!;
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string Role { get; set; } = "User";
    }
}
