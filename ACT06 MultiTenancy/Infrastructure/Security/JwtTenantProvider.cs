using ACT06_MultiTenancy.Application.Interfaces;
using System.Security.Claims;

namespace ACT06_MultiTenancy.Infrastructure.Security
{
    public class JwtTenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _http;

        public JwtTenantProvider(IHttpContextAccessor http) => _http = http;

        public string? TenantId =>
            _http.HttpContext?.User?.FindFirstValue("tenantId");
    }
}
