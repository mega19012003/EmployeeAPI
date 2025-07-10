using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class suaenum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "LateAndLeaveEarly", "Đi trễ và về sớm", 0.29999999999999999 });

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 9,
                column: "SalaryMultiplier",
                value: 0.5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "LeaveWithPermission", "Nghỉ có phép", 0.90000000000000002 });

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 9,
                column: "SalaryMultiplier",
                value: 1.0);
        }
    }
}
