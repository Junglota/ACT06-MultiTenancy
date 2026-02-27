using ACT06_MultiTenancy.Application.Interfaces;
using ACT06_MultiTenancy.Domain.Entities;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace ACT06_MultiTenancy.Infrastructure.Security
{
    public class JwtTokenGenerator : ITokenGenerator
    {
        private readonly IConfiguration _config;

        public JwtTokenGenerator(IConfiguration config) => _config = config;

        public string Generate(User user)
        {
            var key = _config["JWT_Key"] ?? throw new InvalidOperationException("JWT_Key no configurada");
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
        {
            new("tenantId", user.TenantId),
            new(ClaimTypes.Role, user.Role),
            new("username", user.Username),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        };

            var token = new JwtSecurityToken(
                issuer: "Api",
                audience: "Api",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
