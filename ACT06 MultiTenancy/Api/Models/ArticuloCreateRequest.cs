namespace ACT06_MultiTenancy.Api.Models
{
    public record ArticuloCreateRequest(
        string Codigo,
        string Nombre,
        string? Descripcion,
        decimal Precio,
        int StockInicial
    );
}
