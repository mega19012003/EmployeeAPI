using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class addCheckinMorningStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payrolls_SalaryRule_SalaryRuleId",
                table: "Payrolls");

            migrationBuilder.DropTable(
                name: "SalaryRule");

            migrationBuilder.AlterColumn<int>(
                name: "SalaryRuleId",
                table: "Payrolls",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "LogStatusConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SalaryMultiplier = table.Column<double>(type: "float", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogStatusConfigs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "LogStatusConfigs",
                columns: new[] { "Id", "Name", "Note", "SalaryMultiplier" },
                values: new object[,]
                {
                    { 0, "OnTime", "Đi đúng giờ", 1.0 },
                    { 1, "Late", "Đi trễ", 0.69999999999999996 },
                    { 2, "LeaveEarly", "Về sớm", 0.69999999999999996 },
                    { 3, "Overtime", "Làm thêm giờ", 1.3 },
                    { 4, "Absent", "Nghỉ không phép", 0.5 },
                    { 5, "LeaveWithPermission", "Nghỉ có phép", 0.90000000000000002 },
                    { 6, "Others", "Khác", 1.0 }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Payrolls_LogStatusConfigs_SalaryRuleId",
                table: "Payrolls",
                column: "SalaryRuleId",
                principalTable: "LogStatusConfigs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payrolls_LogStatusConfigs_SalaryRuleId",
                table: "Payrolls");

            migrationBuilder.DropTable(
                name: "LogStatusConfigs");

            migrationBuilder.AlterColumn<string>(
                name: "SalaryRuleId",
                table: "Payrolls",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "SalaryRule",
                columns: table => new
                {
                    SalaryRuleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CheckinMorningStatus = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Updated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    multiplier = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryRule", x => x.SalaryRuleId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Payrolls_SalaryRule_SalaryRuleId",
                table: "Payrolls",
                column: "SalaryRuleId",
                principalTable: "SalaryRule",
                principalColumn: "SalaryRuleId");
        }
    }
}
