using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GinPair.Migrations
{
    /// <inheritdoc />
    public partial class tonicBrandRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "tonic_brand",
                schema: "gp_schema",
                table: "tonics",
                type: "varchar",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 100,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "tonic_brand",
                schema: "gp_schema",
                table: "tonics",
                type: "varchar",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 100);
        }
    }
}
