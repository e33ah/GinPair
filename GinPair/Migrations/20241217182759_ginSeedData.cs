using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GinPair.Migrations
{
    /// <inheritdoc />
    public partial class ginSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "gp_schema",
                table: "gins",
                columns: new[] { "gin_id", "distillery", "gin_description", "gin_name" },
                values: new object[,]
                {
                    { 1, "Whitley Neill", null, "Rhubarb & Ginger" },
                    { 2, "Whitley Neill", null, "Blood Orange" },
                    { 3, "Caorunn", null, "Scottish Raspberry" },
                    { 4, "Skylark", null, "Lantic" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "gp_schema",
                table: "gins",
                keyColumn: "gin_id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                schema: "gp_schema",
                table: "gins",
                keyColumn: "gin_id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                schema: "gp_schema",
                table: "gins",
                keyColumn: "gin_id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                schema: "gp_schema",
                table: "gins",
                keyColumn: "gin_id",
                keyValue: 4);
        }
    }
}
