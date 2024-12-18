using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GinPair.Migrations
{
    /// <inheritdoc />
    public partial class pairingFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_pairings_gins_paired_gin_gin_id",
                schema: "gp_schema",
                table: "pairings");

            migrationBuilder.DropForeignKey(
                name: "fk_pairings_tonics_paired_tonic_tonic_id",
                schema: "gp_schema",
                table: "pairings");

            migrationBuilder.RenameColumn(
                name: "paired_tonic_tonic_id",
                schema: "gp_schema",
                table: "pairings",
                newName: "tonic_id");

            migrationBuilder.RenameColumn(
                name: "paired_gin_gin_id",
                schema: "gp_schema",
                table: "pairings",
                newName: "gin_id");

            migrationBuilder.RenameIndex(
                name: "ix_pairings_paired_tonic_tonic_id",
                schema: "gp_schema",
                table: "pairings",
                newName: "ix_pairings_tonic_id");

            migrationBuilder.RenameIndex(
                name: "ix_pairings_paired_gin_gin_id",
                schema: "gp_schema",
                table: "pairings",
                newName: "ix_pairings_gin_id");

            migrationBuilder.AddForeignKey(
                name: "fk_pairings_gins_gin_id",
                schema: "gp_schema",
                table: "pairings",
                column: "gin_id",
                principalSchema: "gp_schema",
                principalTable: "gins",
                principalColumn: "gin_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_pairings_tonics_tonic_id",
                schema: "gp_schema",
                table: "pairings",
                column: "tonic_id",
                principalSchema: "gp_schema",
                principalTable: "tonics",
                principalColumn: "tonic_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_pairings_gins_gin_id",
                schema: "gp_schema",
                table: "pairings");

            migrationBuilder.DropForeignKey(
                name: "fk_pairings_tonics_tonic_id",
                schema: "gp_schema",
                table: "pairings");

            migrationBuilder.RenameColumn(
                name: "tonic_id",
                schema: "gp_schema",
                table: "pairings",
                newName: "paired_tonic_tonic_id");

            migrationBuilder.RenameColumn(
                name: "gin_id",
                schema: "gp_schema",
                table: "pairings",
                newName: "paired_gin_gin_id");

            migrationBuilder.RenameIndex(
                name: "ix_pairings_tonic_id",
                schema: "gp_schema",
                table: "pairings",
                newName: "ix_pairings_paired_tonic_tonic_id");

            migrationBuilder.RenameIndex(
                name: "ix_pairings_gin_id",
                schema: "gp_schema",
                table: "pairings",
                newName: "ix_pairings_paired_gin_gin_id");

            migrationBuilder.AddForeignKey(
                name: "fk_pairings_gins_paired_gin_gin_id",
                schema: "gp_schema",
                table: "pairings",
                column: "paired_gin_gin_id",
                principalSchema: "gp_schema",
                principalTable: "gins",
                principalColumn: "gin_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_pairings_tonics_paired_tonic_tonic_id",
                schema: "gp_schema",
                table: "pairings",
                column: "paired_tonic_tonic_id",
                principalSchema: "gp_schema",
                principalTable: "tonics",
                principalColumn: "tonic_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
