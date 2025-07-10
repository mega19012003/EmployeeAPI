using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class suaenum1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "LeaveEarly", "Về sớm", 0.69999999999999996 });

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "LateAndLeaveEarly", "Đi trễ và về sớm", 0.5 });

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "Overtime", "Làm thêm giờ", 1.3 });

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "LateAndOvertime", "Đi trễ và làm thêm giờ", 1.0 });

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 7,
                column: "Note",
                value: "Vắng");

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "OnHoliday", "Làm vào ngày nghỉ", 2.0 });

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "OnHolidayLate", "Đi trễ vào ngày nghỉ", 1.5 });

            migrationBuilder.InsertData(
                table: "LogStatusConfigs",
                columns: new[] { "Id", "Name", "Note", "SalaryMultiplier" },
                values: new object[,]
                {
                    { 10, "OnHolidayLeaveEarly", "Về sớm vào ngày nghỉ", 1.5 },
                    { 11, "OnHolidayOvertime", "Làm thêm giờ vào ngày nghỉ", 3.0 },
                    { 12, "OnHolidayLateAndOvertime", "Đi trễ và làm thêm giờ vào ngày nghỉ", 2.0 },
                    { 13, "OnHolidayLateAndLeaveEarly", "Đi trễ và về sớm vào ngày nghỉ", 2.0 },
                    { 14, "Others", "Khác", 0.5 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "LateOnHoliday", "Đi trễ vào ngày nghỉ lệ", 1.5 });

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "LeaveEarly", "Về sớm", 0.69999999999999996 });

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "OnHoliday", "Làm vào ngày nghỉ", 2.0 });

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "Overtime", "Làm thêm giờ", 1.3 });

            migrationBuilder.UpdateData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: 7,
                column: "Note",
                value: "Nghỉ không phép");

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
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "Others", "Khác", 0.5 });
        }
    }
}
