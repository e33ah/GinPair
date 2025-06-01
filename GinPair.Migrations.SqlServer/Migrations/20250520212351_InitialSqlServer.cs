using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GinPair.Migrations.SqlServer.Migrations {
    /// <inheritdoc />
    public partial class InitialSqlServer : Migration {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder) {
            migrationBuilder.EnsureSchema(
                name: "gp_schema");

            migrationBuilder.CreateTable(
                name: "Gins",
                schema: "gp_schema",
                columns: table => new {
                    GinId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GinName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Distillery = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GinDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Gins", x => x.GinId);
                });

            migrationBuilder.CreateTable(
                name: "Tonics",
                schema: "gp_schema",
                columns: table => new {
                    TonicId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TonicBrand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TonicFlavour = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Tonics", x => x.TonicId);
                });

            migrationBuilder.CreateTable(
                name: "Pairings",
                schema: "gp_schema",
                columns: table => new {
                    PairingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GinId = table.Column<int>(type: "int", nullable: false),
                    TonicId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table => {
                    table.PrimaryKey("PK_Pairings", x => x.PairingId);
                    table.ForeignKey(
                        name: "FK_Pairings_Gins_GinId",
                        column: x => x.GinId,
                        principalSchema: "gp_schema",
                        principalTable: "Gins",
                        principalColumn: "GinId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pairings_Tonics_TonicId",
                        column: x => x.TonicId,
                        principalSchema: "gp_schema",
                        principalTable: "Tonics",
                        principalColumn: "TonicId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "gp_schema",
                table: "Gins",
                columns: new[] { "GinId", "Distillery", "GinDescription", "GinName" },
                values: new object[,]
                {
                    { 1, "Whitley Neill", null, "Rhubarb & Ginger" },
                    { 2, "Whitley Neill", null, "Blood Orange" },
                    { 3, "Caorunn", null, "Scottish Raspberry" },
                    { 4, "Skylark", null, "Lantic" }
                });

            migrationBuilder.InsertData(
                schema: "gp_schema",
                table: "Tonics",
                columns: new[] { "TonicId", "TonicBrand", "TonicFlavour" },
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
                table: "Pairings",
                columns: new[] { "PairingId", "GinId", "TonicId" },
                values: new object[,]
                {
                    { 1, 1, 4 },
                    { 2, 2, 3 },
                    { 3, 3, 3 },
                    { 4, 4, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pairings_GinId",
                schema: "gp_schema",
                table: "Pairings",
                column: "GinId");

            migrationBuilder.CreateIndex(
                name: "IX_Pairings_TonicId",
                schema: "gp_schema",
                table: "Pairings",
                column: "TonicId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable(
                name: "Pairings",
                schema: "gp_schema");

            migrationBuilder.DropTable(
                name: "Gins",
                schema: "gp_schema");

            migrationBuilder.DropTable(
                name: "Tonics",
                schema: "gp_schema");
        }
    }
}
