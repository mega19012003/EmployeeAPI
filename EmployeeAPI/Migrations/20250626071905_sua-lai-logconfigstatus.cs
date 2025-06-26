using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class sualailogconfigstatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "Holiday", "Làm vào ngày nghỉ", 2.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "LeaveEarly", "Về sớm", 0.69999999999999996 });
        }
    }
}
