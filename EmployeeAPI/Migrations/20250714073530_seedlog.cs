using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmployeeAPI.Migrations
{
    /// <inheritdoc />
    public partial class seedlog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "LogStatusConfigs",
                columns: new[] { "Id", "CompanyId", "CompanyName", "IsSystemDefault", "Name", "Note", "SalaryMultiplier", "enumId" },
                values: new object[,]
                {
                    { new Guid("08eafcc1-dc50-4c78-8cbe-7d07a760b3b7"), null, null, true, "Late", "Đi trễ", 0.69999999999999996, 2 },
                    { new Guid("0c705b2a-9d65-4f53-9899-600b679459f6"), null, null, true, "Absent", "Vắng", 0.0, 7 },
                    { new Guid("149c3474-8834-416a-a025-b4239a5146a3"), null, null, true, "OnHolidayLateAndOvertime", "Đi trễ và làm thêm giờ vào ngày nghỉ", 1.5, 12 },
                    { new Guid("74d5dd05-15f0-47cc-847f-f9daa1f05b0f"), null, null, true, "OnTime", "Đi đúng giờ", 1.0, 1 },
                    { new Guid("7b0d6f5a-ac95-45e2-b38d-7c3a51c6ab98"), null, null, true, "LateAndOvertime", "Đi trễ và làm thêm giờ", 0.69999999999999996, 6 },
                    { new Guid("871473b1-5528-4870-be86-fa2b8afbad79"), null, null, true, "Overtime", "Làm thêm giờ", 1.3, 5 },
                    { new Guid("8b08faba-35d3-4951-9db9-10bbcac4cfea"), null, null, true, "LateAndLeaveEarly", "Đi trễ và về sớm", 0.5, 4 },
                    { new Guid("a356d277-d902-4ee4-abea-209475628832"), null, null, true, "None", "Chưa checkin/checkout", 0.0, 0 },
                    { new Guid("a92a985a-d572-4e0c-aaf2-82d088d53deb"), null, null, true, "OnHolidayLate", "Đi trễ vào ngày nghỉ", 1.5, 9 },
                    { new Guid("b450d4b9-e3d1-4463-ba19-a715fbcc700b"), null, null, true, "OnHolidayLateAndLeaveEarly", "Đi trễ và về sớm vào ngày nghỉ", 2.0, 13 },
                    { new Guid("b71749b1-0575-42cb-9016-3394d2e9844f"), null, null, false, "Others", "Khác", 0.5, 14 },
                    { new Guid("ce6a7402-d9a1-416e-a0de-06b3738d0e70"), null, null, true, "OnHoliday", "Làm vào ngày nghỉ", 2.0, 8 },
                    { new Guid("d4389218-f965-4803-a87a-d3c9cbf3ba8c"), null, null, true, "OnHolidayOvertime", "Làm thêm giờ vào ngày nghỉ", 3.0, 11 },
                    { new Guid("da365ea0-63d5-43c1-9041-9ca1035c2e0c"), null, null, true, "LeaveEarly", "Về sớm", 0.69999999999999996, 3 },
                    { new Guid("f1be3ddd-e335-408c-bd70-c06586140130"), null, null, true, "OnHolidayLeaveEarly", "Về sớm vào ngày nghỉ", 1.5, 10 }
                });

            migrationBuilder.InsertData(
                table: "ScheduleTimes",
                columns: new[] { "id", "CompanyId", "EndTimeAfternoon", "EndTimeMorning", "IsSystemDefault", "LogAllowtime", "StartTimeAfternoon", "StartTimeMorning" },
                values: new object[] { new Guid("bf10b6e0-aefa-4e6e-989a-a2fc7ec12890"), null, new TimeOnly(17, 0, 0), new TimeOnly(12, 0, 0), true, 5, new TimeOnly(13, 0, 0), new TimeOnly(8, 0, 0) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("08eafcc1-dc50-4c78-8cbe-7d07a760b3b7"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("0c705b2a-9d65-4f53-9899-600b679459f6"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("149c3474-8834-416a-a025-b4239a5146a3"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("74d5dd05-15f0-47cc-847f-f9daa1f05b0f"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("7b0d6f5a-ac95-45e2-b38d-7c3a51c6ab98"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("871473b1-5528-4870-be86-fa2b8afbad79"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("8b08faba-35d3-4951-9db9-10bbcac4cfea"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a356d277-d902-4ee4-abea-209475628832"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("a92a985a-d572-4e0c-aaf2-82d088d53deb"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("b450d4b9-e3d1-4463-ba19-a715fbcc700b"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("b71749b1-0575-42cb-9016-3394d2e9844f"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("ce6a7402-d9a1-416e-a0de-06b3738d0e70"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("d4389218-f965-4803-a87a-d3c9cbf3ba8c"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("da365ea0-63d5-43c1-9041-9ca1035c2e0c"));

            migrationBuilder.DeleteData(
                table: "LogStatusConfigs",
                keyColumn: "Id",
                keyValue: new Guid("f1be3ddd-e335-408c-bd70-c06586140130"));

            migrationBuilder.DeleteData(
                table: "ScheduleTimes",
                keyColumn: "id",
                keyValue: new Guid("bf10b6e0-aefa-4e6e-989a-a2fc7ec12890"));
        }
    }
}
