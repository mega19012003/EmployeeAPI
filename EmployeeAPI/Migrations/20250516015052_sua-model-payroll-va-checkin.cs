using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class suamodelpayrollvacheckin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Checkins_Payrolls_PayrollId",
                table: "Checkins");

            migrationBuilder.DropIndex(
                name: "IX_Checkins_PayrollId",
                table: "Checkins");

            migrationBuilder.DropColumn(
                name: "PayrollId",
                table: "Checkins");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PayrollId",
                table: "Checkins",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Checkins_PayrollId",
                table: "Checkins",
                column: "PayrollId");

            migrationBuilder.AddForeignKey(
                name: "FK_Checkins_Payrolls_PayrollId",
                table: "Checkins",
                column: "PayrollId",
                principalTable: "Payrolls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
