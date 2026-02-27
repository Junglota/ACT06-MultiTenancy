using ACT06_MultiTenancy.Api.Models;
using ACT06_MultiTenancy.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _users;
    private readonly ITokenGenerator _tokens;
    private readonly Serilog.ILogger _log;

    public AuthController(IUserRepository users, ITokenGenerator tokens)
    {
        _users = users;
        _tokens = tokens;
        _log = Log.ForContext<AuthController>();
    }

    [HttpPost("/Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await _users.GetByUsernameAsync(req.Username, ct);
        if (user is null) return Unauthorized("Credenciales inválidas");

        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized("Credenciales inválidas");

        var token = _tokens.Generate(user);
        return Ok(new { token });
    }

    [Authorize]
    [HttpPost("/CambioDeClave")]
    public async Task<IActionResult> CambioDeClave([FromBody] ChangePasswordRequest req, CancellationToken ct)
    {
        var tokenUsername = User.FindFirst("username")?.Value;
        if (!string.Equals(tokenUsername, req.Username, StringComparison.OrdinalIgnoreCase))
            return Forbid();

        var user = await _users.GetByUsernameAsync(req.Username, ct);
        if (user is null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
            return BadRequest("Contraseña actual incorrecta");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        await _users.UpdateAsync(user, ct);

        return Ok("Contraseña actualizada");
    }

    [HttpPost("/OlvideMiClave")]
    public IActionResult OlvideMiClave([FromBody] ForgotPasswordRequest req)
    {
        var code = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        _log.Information("Solicitud OlvideMiClave para {User}. Codigo={Code}", req.UsernameOrEmail, code);

        return Ok("Si el usuario existe, se enviará un código de recuperación (simulado).");
    }
}