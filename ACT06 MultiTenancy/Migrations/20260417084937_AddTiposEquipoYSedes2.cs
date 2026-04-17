using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACT06_MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class AddTiposEquipoYSedes2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstadoOperativo",
                table: "Articulos",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Disponible");

            migrationBuilder.AddColumn<Guid>(
                name: "SedeId",
                table: "Articulos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TipoEquipoId",
                table: "Articulos",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Sedes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sedes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposEquipo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "text", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposEquipo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articulos_SedeId",
                table: "Articulos",
                column: "SedeId");

            migrationBuilder.CreateIndex(
                name: "IX_Articulos_TipoEquipoId",
                table: "Articulos",
                column: "TipoEquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_Sedes_TenantId_Nombre",
                table: "Sedes",
                columns: new[] { "TenantId", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposEquipo_TenantId_Nombre",
                table: "TiposEquipo",
                columns: new[] { "TenantId", "Nombre" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Articulos_Sedes_SedeId",
                table: "Articulos",
                column: "SedeId",
                principalTable: "Sedes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Articulos_TiposEquipo_TipoEquipoId",
                table: "Articulos",
                column: "TipoEquipoId",
                principalTable: "TiposEquipo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Articulos_Sedes_SedeId",
                table: "Articulos");

            migrationBuilder.DropForeignKey(
                name: "FK_Articulos_TiposEquipo_TipoEquipoId",
                table: "Articulos");

            migrationBuilder.DropTable(
                name: "Sedes");

            migrationBuilder.DropTable(
                name: "TiposEquipo");

            migrationBuilder.DropIndex(
                name: "IX_Articulos_SedeId",
                table: "Articulos");

            migrationBuilder.DropIndex(
                name: "IX_Articulos_TipoEquipoId",
                table: "Articulos");

            migrationBuilder.DropColumn(
                name: "EstadoOperativo",
                table: "Articulos");

            migrationBuilder.DropColumn(
                name: "SedeId",
                table: "Articulos");

            migrationBuilder.DropColumn(
                name: "TipoEquipoId",
                table: "Articulos");
        }
    }
}
