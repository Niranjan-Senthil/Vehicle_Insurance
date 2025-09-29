using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleInsuranceProject.Repository.Migrations
{
    /// <inheritdoc />
    public partial class addednewpropertyinapplicationdbcontext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Policies_policyId1",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_policyId1",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "policyId1",
                table: "Claims");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "policyId1",
                table: "Claims",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Claims_policyId1",
                table: "Claims",
                column: "policyId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Policies_policyId1",
                table: "Claims",
                column: "policyId1",
                principalTable: "Policies",
                principalColumn: "policyId");
        }
    }
}
