using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACT06_MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class UpdateArticulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Precio",
                table: "Articulos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Precio",
                table: "Articulos",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
