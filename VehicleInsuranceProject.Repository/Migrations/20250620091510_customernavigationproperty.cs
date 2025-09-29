using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleInsuranceProject.Repository.Migrations
{
    /// <inheritdoc />
    public partial class customernavigationproperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
