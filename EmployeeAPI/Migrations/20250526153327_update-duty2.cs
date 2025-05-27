using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class updateduty2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DutyDetail_Duties_DutyId",
                table: "DutyDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_DutyDetail_Users_UserId",
                table: "DutyDetail");

            migrationBuilder.AddColumn<Guid>(
                name: "AssignedById",
                table: "Duties",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Duties",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Duties_AssignedById",
                table: "Duties",
                column: "AssignedById");

            migrationBuilder.CreateIndex(
                name: "IX_Duties_UserId",
                table: "Duties",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Duties_Users_AssignedById",
                table: "Duties",
                column: "AssignedById",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Duties_Users_UserId",
                table: "Duties",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DutyDetail_Duties_DutyId",
                table: "DutyDetail",
                column: "DutyId",
                principalTable: "Duties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DutyDetail_Users_UserId",
                table: "DutyDetail",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Duties_Users_AssignedById",
                table: "Duties");

            migrationBuilder.DropForeignKey(
                name: "FK_Duties_Users_UserId",
                table: "Duties");

            migrationBuilder.DropForeignKey(
                name: "FK_DutyDetail_Duties_DutyId",
                table: "DutyDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_DutyDetail_Users_UserId",
                table: "DutyDetail");

            migrationBuilder.DropIndex(
                name: "IX_Duties_AssignedById",
                table: "Duties");

            migrationBuilder.DropIndex(
                name: "IX_Duties_UserId",
                table: "Duties");

            migrationBuilder.DropColumn(
                name: "AssignedById",
                table: "Duties");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Duties");

            migrationBuilder.AddForeignKey(
                name: "FK_DutyDetail_Duties_DutyId",
                table: "DutyDetail",
                column: "DutyId",
                principalTable: "Duties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DutyDetail_Users_UserId",
                table: "DutyDetail",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
