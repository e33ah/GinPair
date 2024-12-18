using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GinPair.Migrations
{
    /// <inheritdoc />
    public partial class pairingSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "gp_schema",
                table: "pairings",
                columns: new[] { "pairing_id", "gin_id", "tonic_id" },
                values: new object[,]
                {
                    { 1, 1, 4 },
                    { 2, 2, 3 },
                    { 3, 3, 3 },
                    { 4, 4, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "gp_schema",
                table: "pairings",
                keyColumn: "pairing_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "gp_schema",
                table: "pairings",
                keyColumn: "pairing_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "gp_schema",
                table: "pairings",
                keyColumn: "pairing_id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "gp_schema",
                table: "pairings",
                keyColumn: "pairing_id",
                keyValue: 4);
        }
    }
}
