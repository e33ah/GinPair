using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GinPair.Migrations
{
    /// <inheritdoc />
    public partial class pairingsClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pairings",
                schema: "gp_schema",
                columns: table => new
                {
                    pairing_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    paired_gin_gin_id = table.Column<int>(type: "integer", nullable: false),
                    paired_tonic_tonic_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pairings", x => x.pairing_id);
                    table.ForeignKey(
                        name: "fk_pairings_gins_paired_gin_gin_id",
                        column: x => x.paired_gin_gin_id,
                        principalSchema: "gp_schema",
                        principalTable: "gins",
                        principalColumn: "gin_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_pairings_tonics_paired_tonic_tonic_id",
                        column: x => x.paired_tonic_tonic_id,
                        principalSchema: "gp_schema",
                        principalTable: "tonics",
                        principalColumn: "tonic_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pairings_paired_gin_gin_id",
                schema: "gp_schema",
                table: "pairings",
                column: "paired_gin_gin_id");

            migrationBuilder.CreateIndex(
                name: "ix_pairings_paired_tonic_tonic_id",
                schema: "gp_schema",
                table: "pairings",
                column: "paired_tonic_tonic_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pairings",
                schema: "gp_schema");
        }
    }
}
