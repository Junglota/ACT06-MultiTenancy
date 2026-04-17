namespace ACT06_MultiTenancy.Api.DTos
{
    public class EquiposDto
    {
        public class EquiposResponseDto
        {
            public EquiposFiltersDto Filters { get; set; } = new();
            public List<EquipoCardDto> Items { get; set; } = new();
        }

        public class EquiposFiltersDto
        {
            public List<TipoEquipoFilterDto> Tipos { get; set; } = new();
            public List<SedeFilterDto> Sedes { get; set; } = new();
            public List<string> Estados { get; set; } = new();
        }

        public class TipoEquipoFilterDto
        {
            public Guid Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
        }

        public class SedeFilterDto
        {
            public Guid Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
        }

        public class EquipoCardDto
        {
            public Guid Id { get; set; }
            public string Codigo { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
            public decimal Precio { get; set; }

            public string TipoEquipo { get; set; } = string.Empty;
            public string? Sede { get; set; }

            public int Stock { get; set; }
            public int PrestamosActivos { get; set; }

            public string Estado { get; set; } = string.Empty;
        }
    }
}
