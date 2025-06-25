using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class addholiday : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    startDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    endDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "LeaveEarly", "Về sớm", 0.69999999999999996 });

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "Overtime", "Làm thêm giờ", 1.3 });

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "Absent", "Nghỉ không phép", 0.5 });

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "LeaveWithPermission", "Nghỉ có phép", 0.90000000000000002 });

            migrationBuilder.InsertData(
                table: "LogStatusConfigs",
                columns: new[] { "Id", "Name", "Note", "SalaryMultiplier" },
                values: new object[] { 6, "Others", "Khác", 1.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Holidays");

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "Overtime", "Làm thêm giờ", 1.3 });

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "Absent", "Nghỉ không phép", 0.5 });

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "LeaveWithPermission", "Nghỉ có phép", 0.90000000000000002 });

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "Others", "Khác", 1.0 });
        }
    }
}
