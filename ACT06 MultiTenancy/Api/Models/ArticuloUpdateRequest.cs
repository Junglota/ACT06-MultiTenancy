namespace ACT06_MultiTenancy.Api.Models
{
    public record ArticuloUpdateRequest(
        string Codigo,
        string Nombre,
        string? Descripcion,
        decimal Precio
    );
}
