using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleInsuranceProject.Repository.Migrations
{
    /// <inheritdoc />
    public partial class addedcoverageleveltableandaddedcoverageidinpolicymodelandvehiclenavigationproperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Customers_CustomerId1",
                table: "Vehicles");

            migrationBuilder.DropIndex(
                name: "IX_Vehicles_CustomerId1",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "CustomerId1",
                table: "Vehicles");

            migrationBuilder.AddColumn<int>(
                name: "CoverageLevelId",
                table: "Policies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CoverageLevels",
                columns: table => new
                {
                    CoverageLevelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PremiumMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CoverageMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoverageLevels", x => x.CoverageLevelId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Policies_CoverageLevelId",
                table: "Policies",
                column: "CoverageLevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_CoverageLevels_CoverageLevelId",
                table: "Policies",
                column: "CoverageLevelId",
                principalTable: "CoverageLevels",
                principalColumn: "CoverageLevelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policies_CoverageLevels_CoverageLevelId",
                table: "Policies");

            migrationBuilder.DropTable(
                name: "CoverageLevels");

            migrationBuilder.DropIndex(
                name: "IX_Policies_CoverageLevelId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "CoverageLevelId",
                table: "Policies");

            migrationBuilder.AddColumn<int>(
                name: "CustomerId1",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_CustomerId1",
                table: "Vehicles",
                column: "CustomerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Customers_CustomerId1",
                table: "Vehicles",
                column: "CustomerId1",
                principalTable: "Customers",
                principalColumn: "CustomerId");
        }
    }
}
