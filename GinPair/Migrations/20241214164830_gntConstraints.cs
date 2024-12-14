using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GinPair.Migrations
{
    /// <inheritdoc />
    public partial class gntConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "tonic_flavour",
                schema: "gp_schema",
                table: "tonics",
                type: "varchar",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "tonic_brand",
                schema: "gp_schema",
                table: "tonics",
                type: "varchar",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "gin_name",
                schema: "gp_schema",
                table: "gins",
                type: "varchar",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "gin_description",
                schema: "gp_schema",
                table: "gins",
                type: "varchar",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "distillery",
                schema: "gp_schema",
                table: "gins",
                type: "varchar",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "tonic_flavour",
                schema: "gp_schema",
                table: "tonics",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "tonic_brand",
                schema: "gp_schema",
                table: "tonics",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "gin_name",
                schema: "gp_schema",
                table: "gins",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "gin_description",
                schema: "gp_schema",
                table: "gins",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "distillery",
                schema: "gp_schema",
                table: "gins",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
