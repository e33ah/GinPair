using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GinPair.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "gp_schema");

            migrationBuilder.CreateTable(
                name: "gins",
                schema: "gp_schema",
                columns: table => new
                {
                    gin_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gin_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    distillery = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    gin_description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gins", x => x.gin_id);
                });

            migrationBuilder.CreateTable(
                name: "tonics",
                schema: "gp_schema",
                columns: table => new
                {
                    tonic_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tonic_brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tonic_flavour = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tonics", x => x.tonic_id);
                });

            migrationBuilder.CreateTable(
                name: "pairings",
                schema: "gp_schema",
                columns: table => new
                {
                    pairing_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    gin_id = table.Column<int>(type: "integer", nullable: false),
                    tonic_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pairings", x => x.pairing_id);
                    table.ForeignKey(
                        name: "fk_pairings_gins_gin_id",
                        column: x => x.gin_id,
                        principalSchema: "gp_schema",
                        principalTable: "gins",
                        principalColumn: "gin_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pairings_tonics_tonic_id",
                        column: x => x.tonic_id,
                        principalSchema: "gp_schema",
                        principalTable: "tonics",
                        principalColumn: "tonic_id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "ix_pairings_gin_id",
                schema: "gp_schema",
                table: "pairings",
                column: "gin_id");

            migrationBuilder.CreateIndex(
                name: "ix_pairings_tonic_id",
                schema: "gp_schema",
                table: "pairings",
                column: "tonic_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pairings",
                schema: "gp_schema");

            migrationBuilder.DropTable(
                name: "gins",
                schema: "gp_schema");

            migrationBuilder.DropTable(
                name: "tonics",
                schema: "gp_schema");
        }
    }
}
