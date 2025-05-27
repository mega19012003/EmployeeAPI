using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class ipdatepayroll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CheckinStatusConfigs",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "CheckinStatus",
                table: "Payrolls");

            migrationBuilder.UpdateData(
                table: "CheckinStatusConfigs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "Overtime", "Làm thêm giờ", 1.3 });

            migrationBuilder.UpdateData(
                table: "CheckinStatusConfigs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "Absent", "Nghỉ không phép", 0.5 });

            migrationBuilder.UpdateData(
                table: "CheckinStatusConfigs",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "LeaveWithPermission", "Nghỉ có phép", 0.90000000000000002 });

            migrationBuilder.UpdateData(
                table: "CheckinStatusConfigs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "Others", "Khác", 1.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CheckinStatus",
                table: "Payrolls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "CheckinStatusConfigs",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "LeaveEarly", "Về sớm", 0.69999999999999996 });

            migrationBuilder.UpdateData(
                table: "CheckinStatusConfigs",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "Overtime", "Làm thêm giờ", 1.3 });

            migrationBuilder.UpdateData(
                table: "CheckinStatusConfigs",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "Absent", "Nghỉ không phép", 0.5 });

            migrationBuilder.UpdateData(
                table: "CheckinStatusConfigs",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Name", "Note", "SalaryMultiplier" },
                values: new object[] { "LeaveWithPermission", "Nghỉ có phép", 0.90000000000000002 });

            migrationBuilder.InsertData(
                table: "CheckinStatusConfigs",
                columns: new[] { "Id", "Name", "Note", "SalaryMultiplier" },
                values: new object[] { 6, "Others", "Khác", 1.0 });
        }
    }
}
