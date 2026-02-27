using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACT06_MultiTenancy.Migrations
{
    /// <inheritdoc />
    public partial class SeedTestData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tenants
            var tenantA = "TENANT_A";
            var tenantB = "TENANT_B";

            // Users
            var userAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userBId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            // Passwords (BCrypt)
            // password: 123456
            var hash123456 = BCrypt.Net.BCrypt.HashPassword("123456");

            // Notes
            var noteA1 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1");
            var noteA2 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2");
            var noteB1 = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1");

            // Insert Users
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "TenantId", "Username", "Email", "PasswordHash", "Role" },
                values: new object[,]
                {
                { userAId, tenantA, "john", "john@tenantA.com", hash123456, "Admin" },
                { userBId, tenantB, "maria", "maria@tenantB.com", hash123456, "User" }
                });

            // Insert Notes
            migrationBuilder.InsertData(
                table: "Notes",
                columns: new[] { "Id", "TenantId", "Title" },
                values: new object[,]
                {
                { noteA1, tenantA, "Nota A1 - Solo visible en TENANT_A" },
                { noteA2, tenantA, "Nota A2 - Solo visible en TENANT_A" },
                { noteB1, tenantB, "Nota B1 - Solo visible en TENANT_B" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Notes",
                keyColumn: "Id",
                keyValues: new object[]
                {
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1"),
                });

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValues: new object[]
                {
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                });
        }
    }
}
