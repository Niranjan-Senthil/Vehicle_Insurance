using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VehicleInsuranceProject.Repository.Migrations
{
    /// <inheritdoc />
    public partial class addedclaimsmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    claimId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    policyId = table.Column<int>(type: "int", nullable: false),
                    claimAmount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    claimReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    claimDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    claimStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagePaths = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.claimId);
                    table.ForeignKey(
                        name: "FK_Claims_Policies_policyId",
                        column: x => x.policyId,
                        principalTable: "Policies",
                        principalColumn: "policyId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_policyId",
                table: "Claims",
                column: "policyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Claims");
        }
    }
}
