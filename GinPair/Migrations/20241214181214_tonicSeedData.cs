using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GinPair.Migrations;

/// <inheritdoc />
public partial class tonicSeedData : Migration {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) {
        migrationBuilder.InsertData(
            schema: "gp_schema",
            table: "tonics",
            columns: new[] { "tonic_id", "tonic_brand", "tonic_flavour" },
            values: new object[,]
            {
                { 1, "FeverTree", "Indian" },
                { 2, "FeverTree", "Light" },
                { 3, "FeverTree", "Mediterranean" },
                { 4, "FeverTree", "ElderFlower" },
                { 5, "FeverTree", "Aromatic" }
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) {
        migrationBuilder.DeleteData(
            schema: "gp_schema",
            table: "tonics",
            keyColumn: "tonic_id",
            keyValue: 1);

        migrationBuilder.DeleteData(
            schema: "gp_schema",
            table: "tonics",
            keyColumn: "tonic_id",
            keyValue: 2);

        migrationBuilder.DeleteData(
            schema: "gp_schema",
            table: "tonics",
            keyColumn: "tonic_id",
            keyValue: 3);

        migrationBuilder.DeleteData(
            schema: "gp_schema",
            table: "tonics",
            keyColumn: "tonic_id",
            keyValue: 4);

        migrationBuilder.DeleteData(
            schema: "gp_schema",
            table: "tonics",
            keyColumn: "tonic_id",
            keyValue: 5);
    }
}
