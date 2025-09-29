using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleInsuranceProject.Repository.Migrations
{
    /// <inheritdoc />
    public partial class addednavigationpropertyforpolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "premiumAmount",
                table: "Policies",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<string>(
                name: "policyNumber",
                table: "Policies",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<decimal>(
                name: "coverageAmount",
                table: "Policies",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AddColumn<DateTime>(
                name: "PolicyDate",
                table: "Policies",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Policies_policyId1",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_policyId1",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "PolicyDate",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "policyId1",
                table: "Claims");

            migrationBuilder.AlterColumn<decimal>(
                name: "premiumAmount",
                table: "Policies",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "policyNumber",
                table: "Policies",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<decimal>(
                name: "coverageAmount",
                table: "Policies",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
