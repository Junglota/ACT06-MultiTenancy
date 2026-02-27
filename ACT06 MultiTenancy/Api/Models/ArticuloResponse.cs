namespace ACT06_MultiTenancy.Api.Models
{
    public record ArticuloResponse(
        Guid Id,
        string Codigo,
        string Nombre,
        string? Descripcion,
        decimal Precio,
        int Stock
    );
}
