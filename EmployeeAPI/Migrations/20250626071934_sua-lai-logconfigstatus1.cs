using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class sualailogconfigstatus1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "OnHoliday");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Holiday");
        }
    }
}
