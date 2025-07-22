using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class demo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Duties_Companies_CompanyId",
                table: "Duties");

            migrationBuilder.DropForeignKey(
                name: "FK_Duties_Users_AssignedById",
                table: "Duties");

            migrationBuilder.DropForeignKey(
                name: "FK_Duties_Users_UserId",
                table: "Duties");

            migrationBuilder.DropForeignKey(
                name: "FK_DutyDetails_Duties_DutyId",
                table: "DutyDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_DutyDetails_Users_UserId",
                table: "DutyDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DutyDetails",
                table: "DutyDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Duties",
                table: "Duties");

            migrationBuilder.RenameTable(
                name: "DutyDetails",
                newName: "DutyDetail");

            migrationBuilder.RenameTable(
                name: "Duties",
                newName: "Duty");

            migrationBuilder.RenameIndex(
                name: "IX_DutyDetails_UserId",
                table: "DutyDetail",
                newName: "IX_DutyDetail_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_DutyDetails_DutyId",
                table: "DutyDetail",
                newName: "IX_DutyDetail_DutyId");

            migrationBuilder.RenameIndex(
                name: "IX_Duties_UserId",
                table: "Duty",
                newName: "IX_Duty_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Duties_CompanyId",
                table: "Duty",
                newName: "IX_Duty_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_Duties_AssignedById",
                table: "Duty",
                newName: "IX_Duty_AssignedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DutyDetail",
                table: "DutyDetail",
                column: "DutyDetailId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Duty",
                table: "Duty",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Duty_Companies_CompanyId",
                table: "Duty",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Duty_Users_AssignedById",
                table: "Duty",
                column: "AssignedById",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Duty_Users_UserId",
                table: "Duty",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DutyDetail_Duty_DutyId",
                table: "DutyDetail",
                column: "DutyId",
                principalTable: "Duty",
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
                name: "FK_Duty_Companies_CompanyId",
                table: "Duty");

            migrationBuilder.DropForeignKey(
                name: "FK_Duty_Users_AssignedById",
                table: "Duty");

            migrationBuilder.DropForeignKey(
                name: "FK_Duty_Users_UserId",
                table: "Duty");

            migrationBuilder.DropForeignKey(
                name: "FK_DutyDetail_Duty_DutyId",
                table: "DutyDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_DutyDetail_Users_UserId",
                table: "DutyDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DutyDetail",
                table: "DutyDetail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Duty",
                table: "Duty");

            migrationBuilder.RenameTable(
                name: "DutyDetail",
                newName: "DutyDetails");

            migrationBuilder.RenameTable(
                name: "Duty",
                newName: "Duties");

            migrationBuilder.RenameIndex(
                name: "IX_DutyDetail_UserId",
                table: "DutyDetails",
                newName: "IX_DutyDetails_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_DutyDetail_DutyId",
                table: "DutyDetails",
                newName: "IX_DutyDetails_DutyId");

            migrationBuilder.RenameIndex(
                name: "IX_Duty_UserId",
                table: "Duties",
                newName: "IX_Duties_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Duty_CompanyId",
                table: "Duties",
                newName: "IX_Duties_CompanyId");

            migrationBuilder.RenameIndex(
                name: "IX_Duty_AssignedById",
                table: "Duties",
                newName: "IX_Duties_AssignedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DutyDetails",
                table: "DutyDetails",
                column: "DutyDetailId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Duties",
                table: "Duties",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Duties_Companies_CompanyId",
                table: "Duties",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id");

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
    }
}
