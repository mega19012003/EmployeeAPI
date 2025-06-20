using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class fix_db : Migration
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

            migrationBuilder.DropPrimaryKey(
                name: "PK_DutyDetail",
                table: "DutyDetail");

            migrationBuilder.RenameTable(
                name: "DutyDetail",
                newName: "DutyDetails");

            migrationBuilder.RenameIndex(
                name: "IX_DutyDetail_UserId",
                table: "DutyDetails",
                newName: "IX_DutyDetails_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_DutyDetail_DutyId",
                table: "DutyDetails",
                newName: "IX_DutyDetails_DutyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DutyDetails",
                table: "DutyDetails",
                column: "DutyDetailId");

            migrationBuilder.AddForeignKey(
                name: "FK_DutyDetails_Duties_DutyId",
                table: "DutyDetails",
                column: "DutyId",
                principalTable: "Duties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DutyDetails_Users_UserId",
                table: "DutyDetails",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DutyDetails_Duties_DutyId",
                table: "DutyDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_DutyDetails_Users_UserId",
                table: "DutyDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DutyDetails",
                table: "DutyDetails");

            migrationBuilder.RenameTable(
                name: "DutyDetails",
                newName: "DutyDetail");

            migrationBuilder.RenameIndex(
                name: "IX_DutyDetails_UserId",
                table: "DutyDetail",
                newName: "IX_DutyDetail_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_DutyDetails_DutyId",
                table: "DutyDetail",
                newName: "IX_DutyDetail_DutyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DutyDetail",
                table: "DutyDetail",
                column: "DutyDetailId");

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
    }
}
