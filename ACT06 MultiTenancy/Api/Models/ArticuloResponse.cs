namespace ACT06_MultiTenancy.Api.Models
{
    public record ArticuloResponse(
        string Codigo,
        string Nombre,
        string? Descripcion,
        int Stock
    );
}
